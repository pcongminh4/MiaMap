namespace Domain.Places;

public sealed class MenuItem
{
	public MenuItem(int placeId, string name, decimal price, string? description = null)
	{
		PlaceId = placeId;
		Name = name;
		Price = price;
		Description = description;
		CreatedAtUtc = DateTime.UtcNow;
	}

	private MenuItem()
	{
	}

	public int Id { get; private set; }

	public int PlaceId { get; private set; }

	public Place Place { get; private set; } = null!;

	public string Name { get; private set; } = string.Empty;

	public string? Description { get; private set; }

	public decimal Price { get; private set; }

	public DateTime CreatedAtUtc { get; private set; }

	public void UpdateInfo(string name, decimal price, string? description = null)
	{
		Name = name.Trim();
		Price = price;
		Description = description?.Trim();
	}
}
