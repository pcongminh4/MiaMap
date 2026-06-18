using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common.Abstractions.Data;
using Application.Results;
using Domain.Places;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;

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
		logger.LogInformation("Clearing existing OSM data and staging tables for a clean import in area {AreaName}...", area.Name);
		using (var scope = scopeFactory.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			
			// Clear staging tables first
			await dbContext.RawOsmPlaces.ExecuteDeleteAsync(cancellationToken);
			await dbContext.RawOsmWays.ExecuteDeleteAsync(cancellationToken);
			await dbContext.RawOsmNodes.ExecuteDeleteAsync(cancellationToken);

			// Delete roads first because of foreign key references from roads to nodes
			var deletedRoadsCount = await dbContext.Roads.Where(r => r.Source == "osm").ExecuteDeleteAsync(cancellationToken);
			var deletedNodesCount = await dbContext.Nodes.Where(n => n.Source == "osm").ExecuteDeleteAsync(cancellationToken);
			
			// Reset nearest node IDs of places to null
			var updatedPlacesCount = await dbContext.Places.Where(p => p.Source == "osm").ExecuteUpdateAsync(
				s => s.SetProperty(p => p.NearestNodeId, (int?)null), 
				cancellationToken);

			logger.LogInformation(
				"Cleared existing data: deletedRoads={DeletedRoads}, deletedNodes={DeletedNodes}, updatedPlaces={UpdatedPlaces}.",
				deletedRoadsCount,
				deletedNodesCount,
				updatedPlacesCount);
		}

		await ImportPlacesAsync(area, cancellationToken);
		await ImportRoadsAsync(area, cancellationToken);
		await LinkPlacesToNearestNodesAsync(area, cancellationToken);
	}

	private async Task ImportPlacesAsync(OpenStreetMapImportArea area, CancellationToken cancellationToken)
	{
		var payload = await ExecuteOverpassQueryAsync(BuildPlaceOverpassQuery(area), cancellationToken);

		var rawPlaces = payload.Elements
			.Where(element => element.Type == "node")
			.Where(element => element.Latitude.HasValue && element.Longitude.HasValue)
			.Where(element => !string.IsNullOrWhiteSpace(element.Tags.Name))
			.Select(element => new RawOsmPlace
			{
				Id = element.Id,
				Location = new Point(element.Longitude!.Value, element.Latitude!.Value) { SRID = 4326 },
				Name = element.Tags.Name ?? string.Empty,
				Category = ResolveCategory(element.Tags),
				Address = ResolveAddress(element.Tags),
				Tags = JsonSerializer.Serialize(element.Tags.ToDictionary(), JsonOptions)
			})
			.ToList();

		if (rawPlaces.Count == 0)
		{
			logger.LogInformation("No OpenStreetMap places found for area {AreaName}.", area.Name);
			return;
		}

		using (var scope = scopeFactory.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			await dbContext.RawOsmPlaces.AddRangeAsync(rawPlaces, cancellationToken);
			await dbContext.SaveChangesAsync(cancellationToken);
		}

		List<RawOsmPlace> dbRawPlaces;
		using (var scope = scopeFactory.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			dbRawPlaces = await dbContext.RawOsmPlaces.AsNoTracking().ToListAsync(cancellationToken);
		}

		var created = 0;
		var updated = 0;
		var batchSize = Math.Max(1, _options.BatchSize);

		using var outerScope = scopeFactory.CreateScope();
		var placeRepository = outerScope.ServiceProvider.GetRequiredService<IPlaceRepository>();
		var unitOfWork = outerScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

		foreach (var batch in dbRawPlaces.Chunk(batchSize))
		{
			foreach (var rawPlace in batch)
			{
				var record = new ExternalPlaceUpsertRequest(
					Source: "osm",
					ExternalId: rawPlace.Id.ToString(),
					ExternalType: "node",
					Name: rawPlace.Name,
					Category: rawPlace.Category,
					Address: rawPlace.Address,
					Latitude: rawPlace.Location.Y,
					Longitude: rawPlace.Location.X,
					Tags: rawPlace.Tags);

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
			"Imported OpenStreetMap places for area {AreaName}: total={Total}, created={Created}, updated={Updated}.",
			area.Name,
			dbRawPlaces.Count,
			created,
			updated);
	}

	private async Task ImportRoadsAsync(OpenStreetMapImportArea area, CancellationToken cancellationToken)
	{
		var payload = await ExecuteOverpassQueryAsync(BuildRoadOverpassQuery(area), cancellationToken);

		var rawNodes = payload.Elements
			.Where(element => element.Type == "node")
			.Where(element => element.Latitude.HasValue && element.Longitude.HasValue)
			.Select(element => new RawOsmNode
			{
				Id = element.Id,
				Location = new Point(element.Longitude!.Value, element.Latitude!.Value) { SRID = 4326 },
				Name = element.Tags.Name,
				Tags = JsonSerializer.Serialize(element.Tags.ToDictionary(), JsonOptions)
			})
			.ToList();

		var rawWays = payload.Elements
			.Where(element => element.Type == "way")
			.Where(element => !string.IsNullOrWhiteSpace(element.Tags.Highway))
			.Where(element => element.Nodes.Count >= 2)
			.Select(element => new RawOsmWay
			{
				Id = element.Id,
				NodeIds = element.Nodes.ToArray(),
				Tags = JsonSerializer.Serialize(element.Tags.ToDictionary(), JsonOptions),
				Highway = element.Tags.Highway,
				Name = element.Tags.Name ?? element.Tags.Reference,
				Oneway = element.Tags.Oneway,
				Maxspeed = element.Tags.Maxspeed
			})
			.ToList();

		using (var scope = scopeFactory.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			await dbContext.RawOsmNodes.AddRangeAsync(rawNodes, cancellationToken);
			await dbContext.RawOsmWays.AddRangeAsync(rawWays, cancellationToken);
			await dbContext.SaveChangesAsync(cancellationToken);
		}

		List<RawOsmNode> dbRawNodes;
		List<RawOsmWay> dbRawWays;
		using (var scope = scopeFactory.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			dbRawNodes = await dbContext.RawOsmNodes.AsNoTracking().ToListAsync(cancellationToken);
			dbRawWays = await dbContext.RawOsmWays.AsNoTracking().ToListAsync(cancellationToken);
		}

		if (dbRawWays.Count == 0)
		{
			logger.LogInformation("No OpenStreetMap roads found for area {AreaName}.", area.Name);
			return;
		}

		var rawNodesById = dbRawNodes.ToDictionary(n => n.Id);

		// First, count occurrences of each node ID across all ways.
		var nodeReferenceCounts = new Dictionary<long, int>();
		var endpointNodes = new HashSet<long>();

		foreach (var way in dbRawWays)
		{
			if (way.NodeIds.Length < 2)
			{
				continue;
			}

			endpointNodes.Add(way.NodeIds.First());
			endpointNodes.Add(way.NodeIds.Last());

			foreach (var nodeId in way.NodeIds)
			{
				nodeReferenceCounts[nodeId] = nodeReferenceCounts.GetValueOrDefault(nodeId) + 1;
			}
		}

		// A node is a routing node if it is in endpointNodes OR if it's referenced in >= 2 ways.
		var routingNodeIds = new HashSet<long>();
		foreach (var kv in nodeReferenceCounts)
		{
			if (endpointNodes.Contains(kv.Key) || kv.Value >= 2)
			{
				routingNodeIds.Add(kv.Key);
			}
		}

		var batchSize = Math.Max(1, _options.BatchSize);
		var createdNodes = 0;
		var updatedNodes = 0;
		var createdRoads = 0;
		var updatedRoads = 0;
		var nodeCache = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);
		var roadCache = new Dictionary<string, Road>(StringComparer.OrdinalIgnoreCase);

		using var outerScope = scopeFactory.CreateScope();
		var dbContextMain = outerScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		foreach (var batch in dbRawWays.Chunk(batchSize))
		{
			foreach (var way in batch)
			{
				if (way.NodeIds.Length < 2)
				{
					continue;
				}

				var startIndex = 0;
				while (startIndex < way.NodeIds.Length - 1)
				{
					// Find next routing node
					var nextRoutingIndex = -1;
					for (int j = startIndex + 1; j < way.NodeIds.Length; j++)
					{
						if (routingNodeIds.Contains(way.NodeIds[j]))
						{
							nextRoutingIndex = j;
							break;
						}
					}

					if (nextRoutingIndex == -1)
					{
						nextRoutingIndex = way.NodeIds.Length - 1;
					}

					// Extract nodes for this segment
					var segmentNodeIds = way.NodeIds
						.Skip(startIndex)
						.Take(nextRoutingIndex - startIndex + 1)
						.ToList();

					// Resolve RawOsmNodes
					var segmentNodes = new List<RawOsmNode>();
					var resolvedAll = true;
					foreach (var nodeId in segmentNodeIds)
					{
						if (!rawNodesById.TryGetValue(nodeId, out var rawNode))
						{
							resolvedAll = false;
							break;
						}
						segmentNodes.Add(rawNode);
					}

					if (!resolvedAll || segmentNodes.Count < 2)
					{
						startIndex = nextRoutingIndex;
						continue;
					}

					var startNodeResult = await UpsertNodeFromRawAsync(dbContextMain, nodeCache, segmentNodes.First(), cancellationToken);
					createdNodes += startNodeResult.Created ? 1 : 0;
					updatedNodes += startNodeResult.Created ? 0 : 1;

					var endNodeResult = await UpsertNodeFromRawAsync(dbContextMain, nodeCache, segmentNodes.Last(), cancellationToken);
					createdNodes += endNodeResult.Created ? 1 : 0;
					updatedNodes += endNodeResult.Created ? 0 : 1;

					await dbContextMain.SaveChangesAsync(cancellationToken);

					var coordinates = segmentNodes
						.Select(node => new Coordinate(node.Location.X, node.Location.Y))
						.ToArray();
					var geometry = new LineString(coordinates) { SRID = 4326 };
					var lengthMeters = CalculateLengthMetersFromRaw(segmentNodes);

					var segmentExternalId = $"{way.Id}_{segmentNodes.First().Id}_{segmentNodes.Last().Id}";

					var roadResult = await UpsertRoadFromRawAsync(
						dbContextMain,
						roadCache,
						way,
						segmentExternalId,
						geometry,
						lengthMeters,
						startNodeResult.Node.Id,
						endNodeResult.Node.Id,
						IsTruthy(way.Oneway),
						way.Name,
						way.Highway,
						ParseSpeedLimit(way.Maxspeed),
						cancellationToken);
					createdRoads += roadResult.Created ? 1 : 0;
					updatedRoads += roadResult.Created ? 0 : 1;

					startIndex = nextRoutingIndex;
				}
			}

			await dbContextMain.SaveChangesAsync(cancellationToken);
		}

		logger.LogInformation(
			"Imported OpenStreetMap roads for area {AreaName}: total={Total}, nodesCreated={NodesCreated}, nodesUpdated={NodesUpdated}, roadsCreated={RoadsCreated}, roadsUpdated={RoadsUpdated}.",
			area.Name,
			dbRawWays.Count,
			createdNodes,
			updatedNodes,
			createdRoads,
			updatedRoads);
	}

	private async Task<OverpassResponse> ExecuteOverpassQueryAsync(string query, CancellationToken cancellationToken)
	{
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
		return await JsonSerializer.DeserializeAsync<OverpassResponse>(stream, JsonOptions, cancellationToken)
			?? new OverpassResponse();
	}

	private string BuildPlaceOverpassQuery(OpenStreetMapImportArea area)
	{
		var limit = Math.Max(1, _options.MaxPlacesPerArea);
		return
			"[out:json][timeout:25];" +
			"(" +
			$"node[\"name\"][\"amenity\"]({area.South},{area.West},{area.North},{area.East});" +
			$"node[\"name\"][\"shop\"]({area.South},{area.West},{area.North},{area.East});" +
			$"node[\"name\"][\"tourism\"]({area.South},{area.West},{area.North},{area.East});" +
			")" +
			$";out body {limit};";
	}

	private string BuildRoadOverpassQuery(OpenStreetMapImportArea area)
	{
		return
			"[out:json][timeout:25];" +
			"(" +
			$"way[\"highway\"]({area.South},{area.West},{area.North},{area.East});" +
			")" +
			";out body;>;out skel qt;";
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

	private static async Task<(Node Node, bool Created)> UpsertNodeFromRawAsync(
		ApplicationDbContext dbContext,
		Dictionary<string, Node> nodeCache,
		RawOsmNode rawNode,
		CancellationToken cancellationToken)
	{
		var externalId = rawNode.Id.ToString();
		if (nodeCache.TryGetValue(externalId, out var cachedNode))
		{
			return (cachedNode, false);
		}

		var existingNode = await dbContext.Nodes.SingleOrDefaultAsync(
			node => node.Source == "osm" && node.ExternalId == externalId,
			cancellationToken);

		if (existingNode is null)
		{
			var createdNode = Node.CreateFromExternal(
				"osm",
				externalId,
				rawNode.Location.Y,
				rawNode.Location.X,
				rawNode.Name,
				DateTime.UtcNow);

			await dbContext.Nodes.AddAsync(createdNode, cancellationToken);
			nodeCache[externalId] = createdNode;
			return (createdNode, true);
		}

		existingNode.UpdateFromExternal(
			rawNode.Location.Y,
			rawNode.Location.X,
			rawNode.Name);
		nodeCache[externalId] = existingNode;
		return (existingNode, false);
	}

	private static async Task<(Road Road, bool Created)> UpsertRoadFromRawAsync(
		ApplicationDbContext dbContext,
		Dictionary<string, Road> roadCache,
		RawOsmWay rawWay,
		string externalId,
		LineString geometry,
		double lengthMeters,
		int startNodeId,
		int endNodeId,
		bool isOneWay,
		string? roadName,
		string? roadType,
		int maxSpeedKmh,
		CancellationToken cancellationToken)
	{
		if (roadCache.TryGetValue(externalId, out var cachedRoad))
		{
			return (cachedRoad, false);
		}

		var existingRoad = await dbContext.Roads.SingleOrDefaultAsync(
			road => road.Source == "osm" && road.ExternalId == externalId,
			cancellationToken);

		if (existingRoad is null)
		{
			var createdRoad = Road.CreateFromExternal(
				"osm",
				externalId,
				geometry,
				lengthMeters,
				startNodeId,
				endNodeId,
				isOneWay,
				roadName,
				roadType,
				maxSpeedKmh);

			await dbContext.Roads.AddAsync(createdRoad, cancellationToken);
			roadCache[externalId] = createdRoad;
			return (createdRoad, true);
		}

		existingRoad.UpdateFromExternal(
			geometry,
			lengthMeters,
			startNodeId,
			endNodeId,
			isOneWay,
			roadName,
			roadType,
			maxSpeedKmh);
		roadCache[externalId] = existingRoad;
		return (existingRoad, false);
	}

	private static double CalculateLengthMetersFromRaw(IReadOnlyList<RawOsmNode> pathNodes)
	{
		double total = 0;

		for (var index = 1; index < pathNodes.Count; index++)
		{
			total += HaversineDistanceMeters(
				pathNodes[index - 1].Location.Y,
				pathNodes[index - 1].Location.X,
				pathNodes[index].Location.Y,
				pathNodes[index].Location.X);
		}

		return total;
	}

	private static bool TryBuildRoadGeometry(
		OverpassElement roadElement,
		IReadOnlyDictionary<long, OverpassElement> nodeElementsById,
		out LineString geometry,
		out List<OverpassElement> pathNodes)
	{
		pathNodes = [];
		geometry = null!;

		var resolvedNodes = new List<OverpassElement>();
		foreach (var nodeId in roadElement.Nodes)
		{
			if (!nodeElementsById.TryGetValue(nodeId, out var nodeElement))
			{
				return false;
			}

			resolvedNodes.Add(nodeElement);
		}

		if (resolvedNodes.Count < 2)
		{
			return false;
		}

		var coordinates = resolvedNodes
			.Select(node => new Coordinate(node.Longitude!.Value, node.Latitude!.Value))
			.ToArray();

		geometry = new LineString(coordinates) { SRID = 4326 };
		pathNodes = resolvedNodes;
		return true;
	}

	private static double CalculateLengthMeters(IReadOnlyList<OverpassElement> pathNodes)
	{
		double total = 0;

		for (var index = 1; index < pathNodes.Count; index++)
		{
			total += HaversineDistanceMeters(
				pathNodes[index - 1].Latitude!.Value,
				pathNodes[index - 1].Longitude!.Value,
				pathNodes[index].Latitude!.Value,
				pathNodes[index].Longitude!.Value);
		}

		return total;
	}

	private static double HaversineDistanceMeters(
		double latitude1,
		double longitude1,
		double latitude2,
		double longitude2)
	{
		const double radiusMeters = 6_371_000;
		var latitudeDelta = ToRadians(latitude2 - latitude1);
		var longitudeDelta = ToRadians(longitude2 - longitude1);
		var a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2) +
			Math.Cos(ToRadians(latitude1)) * Math.Cos(ToRadians(latitude2)) *
			Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);
		var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
		return radiusMeters * c;
	}

	private static double ToRadians(double degrees)
	{
		return degrees * Math.PI / 180;
	}

	private static int ParseSpeedLimit(string? maxSpeed)
	{
		if (string.IsNullOrWhiteSpace(maxSpeed))
		{
			return 50;
		}

		var digits = new string(maxSpeed.Where(char.IsDigit).ToArray());
		if (int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
		{
			return parsed;
		}

		return 50;
	}

	private static bool IsTruthy(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}

		return value.Trim().ToLowerInvariant() is "yes" or "true" or "1" or "y" or "t";
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

	private async Task LinkPlacesToNearestNodesAsync(OpenStreetMapImportArea area, CancellationToken cancellationToken)
	{
		logger.LogInformation("Linking places to their nearest nodes using PostGIS index-assisted spatial lookup in area {AreaName}...", area.Name);
		
		using var scope = scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		var linkedCount = await dbContext.Database.ExecuteSqlRawAsync(
			@"UPDATE places p
			  SET nearest_node_id = (
				  SELECT n.id
				  FROM nodes n
				  WHERE n.is_active = true
				  ORDER BY p.point <-> n.location
				  LIMIT 1
			  )
			  WHERE p.is_active = true AND p.nearest_node_id IS NULL AND p.source = 'osm';",
			cancellationToken);

		logger.LogInformation(
			"Linked {LinkedCount} places to nearest nodes in area {AreaName}.",
			linkedCount,
			area.Name);
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

		[JsonPropertyName("nodes")]
		public List<long> Nodes { get; init; } = [];

		[JsonPropertyName("tags")]
		public OverpassTags Tags { get; init; } = new();
	}

	private sealed class OverpassTags
	{
		[JsonPropertyName("name")]
		public string? Name { get; init; }

		[JsonPropertyName("ref")]
		public string? Reference { get; init; }

		[JsonPropertyName("amenity")]
		public string? Amenity { get; init; }

		[JsonPropertyName("shop")]
		public string? Shop { get; init; }

		[JsonPropertyName("tourism")]
		public string? Tourism { get; init; }

		[JsonPropertyName("highway")]
		public string? Highway { get; init; }

		[JsonPropertyName("oneway")]
		public string? Oneway { get; init; }

		[JsonPropertyName("maxspeed")]
		public string? Maxspeed { get; init; }

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
				["ref"] = Reference,
				["amenity"] = Amenity,
				["shop"] = Shop,
				["tourism"] = Tourism,
				["highway"] = Highway,
				["oneway"] = Oneway,
				["maxspeed"] = Maxspeed,
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
