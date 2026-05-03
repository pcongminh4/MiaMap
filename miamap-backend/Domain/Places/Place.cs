using NetTopologySuite.Geometries;

namespace Domain.Places;

public sealed class Place
{
	public Place(
		string name,
		string category,
		double latitude,
		double longitude,
		double rating,
		int reviewCount,
		string? address = null)
	{
		Name = name;
		Category = category;
		Address = address;
		Point = CreatePoint(latitude, longitude);
		Rating = rating;
		ReviewCount = reviewCount;
		CreatedAtUtc = DateTime.UtcNow;
		IsActive = true;
	}

	private Place()
	{
	}

	public int Id { get; private set; }

	public string Name { get; private set; } = string.Empty;

	public string Category { get; private set; } = string.Empty;

	public string? Address { get; private set; }

	public Point Point { get; private set; } = new(0, 0) { SRID = 4326 };

	public double Rating { get; private set; }

	public int ReviewCount { get; private set; }

	public bool IsActive { get; private set; }

	public string? Source { get; private set; }

	public string? ExternalId { get; private set; }

	public string? ExternalType { get; private set; }

	public string? Tags { get; private set; }

	public DateTime? LastSyncedAtUtc { get; private set; }

	public DateTime CreatedAtUtc { get; private set; }

	public DateTime? UpdatedAtUtc { get; private set; }

	public static Place CreateFromExternal(
		string source,
		string externalId,
		string externalType,
		string name,
		string category,
		double latitude,
		double longitude,
		string? address,
		string? tags,
		DateTime syncedAtUtc)
	{
		if (string.IsNullOrWhiteSpace(source))
		{
			throw new ArgumentException("Place source is required.", nameof(source));
		}

		if (string.IsNullOrWhiteSpace(externalId))
		{
			throw new ArgumentException("Place external id is required.", nameof(externalId));
		}

		if (string.IsNullOrWhiteSpace(externalType))
		{
			throw new ArgumentException("Place external type is required.", nameof(externalType));
		}

		var place = new Place(
			name.Trim(),
			category.Trim().ToLowerInvariant(),
			latitude,
			longitude,
			0,
			0,
			string.IsNullOrWhiteSpace(address) ? null : address.Trim());
		place.Source = source.Trim().ToLowerInvariant();
		place.ExternalId = externalId.Trim();
		place.ExternalType = externalType.Trim().ToLowerInvariant();
		place.Tags = string.IsNullOrWhiteSpace(tags) ? null : tags;
		place.LastSyncedAtUtc = syncedAtUtc;

		return place;
	}

	public void UpdateBasicInfo(string name, string category, string? address)
	{
		Name = name.Trim();
		Category = category.Trim().ToLowerInvariant();
		Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
		UpdatedAtUtc = DateTime.UtcNow;
	}

	public void UpdateLocation(double latitude, double longitude)
	{
		Point = CreatePoint(latitude, longitude);
		UpdatedAtUtc = DateTime.UtcNow;
	}

	public void UpdateRating(double rating, int reviewCount)
	{
		Rating = rating;
		ReviewCount = reviewCount;
		UpdatedAtUtc = DateTime.UtcNow;
	}

	public void SetActive(bool isActive)
	{
		IsActive = isActive;
		UpdatedAtUtc = DateTime.UtcNow;
	}

	public void UpdateFromExternal(
		string name,
		string category,
		double latitude,
		double longitude,
		string externalType,
		string? address,
		string? tags,
		DateTime syncedAtUtc)
	{
		if (string.IsNullOrWhiteSpace(externalType))
		{
			throw new ArgumentException("Place external type is required.", nameof(externalType));
		}

		UpdateBasicInfo(name, category, address);
		UpdateLocation(latitude, longitude);
		ExternalType = externalType.Trim().ToLowerInvariant();
		Tags = string.IsNullOrWhiteSpace(tags) ? null : tags;
		LastSyncedAtUtc = syncedAtUtc;
		UpdatedAtUtc = DateTime.UtcNow;
	}

	private static Point CreatePoint(double latitude, double longitude)
	{
		return new Point(longitude, latitude) { SRID = 4326 };
	}
}