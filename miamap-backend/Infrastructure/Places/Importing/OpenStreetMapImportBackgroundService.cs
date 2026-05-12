using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Abstractions.Data;
using Application.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Places.Importing;

public sealed class OpenStreetMapImportBackgroundService(
	IServiceScopeFactory scopeFactory,
	IHttpClientFactory httpClientFactory,
	IOptions<OpenStreetMapImportOptions> options,
	ILogger<OpenStreetMapImportBackgroundService> logger) : BackgroundService
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
	private readonly OpenStreetMapImportOptions _options = options.Value;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (!_options.Enabled)
		{
			logger.LogInformation("OpenStreetMap import is disabled.");
			return;
		}

		if (_options.Areas.Count == 0)
		{
			logger.LogWarning("OpenStreetMap import is enabled but no import areas are configured.");
			return;
		}

		logger.LogInformation("OpenStreetMap batch import started for {AreaCount} areas.", _options.Areas.Count);

		foreach (var area in _options.Areas)
		{
			if (stoppingToken.IsCancellationRequested)
			{
				break;
			}

			try
			{
				await ImportAreaWithRetryAsync(area, stoppingToken);
			}
			catch (Exception exception)
			{
				logger.LogError(exception, "OpenStreetMap import failed for area {AreaName}.", area.Name);
			}
		}

		logger.LogInformation("OpenStreetMap batch import finished.");
	}

	private async Task ImportAreaWithRetryAsync(OpenStreetMapImportArea area, CancellationToken cancellationToken)
	{
		var attempt = 0;
		var maxRetries = Math.Max(1, _options.MaxRetries);

		while (attempt < maxRetries)
		{
			attempt++;

			try
			{
				await ImportAreaAsync(area, cancellationToken);
				return;
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception exception)
			{
				if (attempt >= maxRetries)
				{
					throw;
				}

				var delaySeconds = Math.Max(1, _options.RetryDelaySeconds) * attempt;
				logger.LogWarning(
					exception,
					"OpenStreetMap import retry {Attempt}/{MaxRetries} for area {AreaName} after {DelaySeconds}s.",
					attempt,
					maxRetries,
					area.Name,
					delaySeconds);
				await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
			}
		}
	}

	private async Task ImportAreaAsync(OpenStreetMapImportArea area, CancellationToken cancellationToken)
	{
		var query = BuildOverpassQuery(area);
		var client = httpClientFactory.CreateClient("openstreetmap-import");

		using var request = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl)
		{
			Content = new FormUrlEncodedContent(
			[
				new KeyValuePair<string, string>("data", query)
			])
		};

		request.Headers.UserAgent.ParseAdd("miamap-backend-importer/1.0");

		using var response = await client.SendAsync(request, cancellationToken);
		response.EnsureSuccessStatusCode();

		await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
		var payload = await JsonSerializer.DeserializeAsync<OverpassResponse>(stream, JsonOptions, cancellationToken)
			?? new OverpassResponse();

		var records = payload.Elements
			.Where(element => element.Latitude.HasValue && element.Longitude.HasValue)
			.Where(element => !string.IsNullOrWhiteSpace(element.Tags.Name))
			.Select(ToUpsertRequest)
			.ToList();

		if (records.Count == 0)
		{
			logger.LogInformation("No OpenStreetMap places found for area {AreaName}.", area.Name);
			return;
		}

		var created = 0;
		var updated = 0;
		var batchSize = Math.Max(1, _options.BatchSize);

		using var scope = scopeFactory.CreateScope();
		var placeRepository = scope.ServiceProvider.GetRequiredService<IPlaceRepository>();
		var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

		foreach (var batch in records.Chunk(batchSize))
		{
			foreach (var record in batch)
			{
				var result = await placeRepository.UpsertExternalAsync(record, cancellationToken);

				if (result.Created)
				{
					created++;
				}
				else
				{
					updated++;
				}
			}

			await unitOfWork.SaveChangesAsync(cancellationToken);
		}

		logger.LogInformation(
			"Imported OpenStreetMap area {AreaName}: total={Total}, created={Created}, updated={Updated}.",
			area.Name,
			records.Count,
			created,
			updated);
	}

	private string BuildOverpassQuery(OpenStreetMapImportArea area)
	{
		var limit = Math.Max(1, _options.MaxPlacesPerArea);
		var rawQuery =
			"[out:json][timeout:25];" +
			"(" +
			$"node[\"name\"][\"amenity\"]({area.South},{area.West},{area.North},{area.East});" +
			$"node[\"name\"][\"shop\"]({area.South},{area.West},{area.North},{area.East});" +
			$"node[\"name\"][\"tourism\"]({area.South},{area.West},{area.North},{area.East});" +
			")" +
			$";out body {limit};";

		return rawQuery;
	}

	private static ExternalPlaceUpsertRequest ToUpsertRequest(OverpassElement element)
	{
		var tagsDictionary = element.Tags.ToDictionary();
		var tagsJson = JsonSerializer.Serialize(tagsDictionary, JsonOptions);

		return new ExternalPlaceUpsertRequest(
			Source: "osm",
			ExternalId: element.Id.ToString(),
			ExternalType: element.Type,
			Name: element.Tags.Name ?? string.Empty,
			Category: ResolveCategory(element.Tags),
			Address: ResolveAddress(element.Tags),
			Latitude: element.Latitude ?? 0,
			Longitude: element.Longitude ?? 0,
			Tags: tagsJson);
	}

	private static string ResolveCategory(OverpassTags tags)
	{
		if (!string.IsNullOrWhiteSpace(tags.Amenity))
		{
			return tags.Amenity;
		}

		if (!string.IsNullOrWhiteSpace(tags.Shop))
		{
			return tags.Shop;
		}

		if (!string.IsNullOrWhiteSpace(tags.Tourism))
		{
			return tags.Tourism;
		}

		if (!string.IsNullOrWhiteSpace(tags.Leisure))
		{
			return tags.Leisure;
		}

		return "other";
	}

	private static string? ResolveAddress(OverpassTags tags)
	{
		if (!string.IsNullOrWhiteSpace(tags.FormattedAddress))
		{
			return tags.FormattedAddress;
		}

		var parts = new[] { tags.HouseNumber, tags.Street, tags.City }
			.Where(part => !string.IsNullOrWhiteSpace(part))
			.ToArray();

		return parts.Length == 0 ? null : string.Join(", ", parts);
	}

	private sealed class OverpassResponse
	{
		[JsonPropertyName("elements")]
		public List<OverpassElement> Elements { get; init; } = [];
	}

	private sealed class OverpassElement
	{
		[JsonPropertyName("type")]
		public string Type { get; init; } = string.Empty;

		[JsonPropertyName("id")]
		public long Id { get; init; }

		[JsonPropertyName("lat")]
		public double? Latitude { get; init; }

		[JsonPropertyName("lon")]
		public double? Longitude { get; init; }

		[JsonPropertyName("tags")]
		public OverpassTags Tags { get; init; } = new();
	}

	private sealed class OverpassTags
	{
		[JsonPropertyName("name")]
		public string? Name { get; init; }

		[JsonPropertyName("amenity")]
		public string? Amenity { get; init; }

		[JsonPropertyName("shop")]
		public string? Shop { get; init; }

		[JsonPropertyName("tourism")]
		public string? Tourism { get; init; }

		[JsonPropertyName("leisure")]
		public string? Leisure { get; init; }

		[JsonPropertyName("addr:full")]
		public string? FormattedAddress { get; init; }

		[JsonPropertyName("addr:housenumber")]
		public string? HouseNumber { get; init; }

		[JsonPropertyName("addr:street")]
		public string? Street { get; init; }

		[JsonPropertyName("addr:city")]
		public string? City { get; init; }

		public Dictionary<string, string?> ToDictionary()
		{
			var pairs = new Dictionary<string, string?>
			{
				["name"] = Name,
				["amenity"] = Amenity,
				["shop"] = Shop,
				["tourism"] = Tourism,
				["leisure"] = Leisure,
				["addr:full"] = FormattedAddress,
				["addr:housenumber"] = HouseNumber,
				["addr:street"] = Street,
				["addr:city"] = City
			};

			return pairs
				.Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
				.ToDictionary(pair => pair.Key, pair => pair.Value);
		}
	}
}

