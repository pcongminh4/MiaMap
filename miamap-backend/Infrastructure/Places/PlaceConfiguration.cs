using Domain.Places;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Places;

public sealed class PlaceConfiguration : IEntityTypeConfiguration<Place>
{
	public void Configure(EntityTypeBuilder<Place> builder)
	{
		builder.ToTable("places");

		builder.HasKey(place => place.Id);

		builder.Property(place => place.Id)
			.HasColumnName("id")
			.ValueGeneratedOnAdd();

		builder.Property(place => place.Name)
			.HasColumnName("name")
			.HasMaxLength(200)
			.IsRequired();

		builder.Property(place => place.Category)
			.HasColumnName("category")
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(place => place.Address)
			.HasColumnName("address")
			.HasMaxLength(500);

		builder.Property(place => place.Latitude)
			.HasColumnName("latitude")
			.IsRequired();

		builder.Property(place => place.Longitude)
			.HasColumnName("longitude")
			.IsRequired();

		builder.Property(place => place.Rating)
			.HasColumnName("rating")
			.IsRequired();

		builder.Property(place => place.ReviewCount)
			.HasColumnName("review_count")
			.IsRequired();

		builder.Property(place => place.IsActive)
			.HasColumnName("is_active")
			.IsRequired();

		builder.Property(place => place.Source)
			.HasColumnName("source")
			.HasMaxLength(50);

		builder.Property(place => place.ExternalId)
			.HasColumnName("external_id")
			.HasMaxLength(100);

		builder.Property(place => place.ExternalType)
			.HasColumnName("external_type")
			.HasMaxLength(50);

		builder.Property(place => place.Tags)
			.HasColumnName("tags")
			.HasColumnType("jsonb");

		builder.Property(place => place.LastSyncedAtUtc)
			.HasColumnName("last_synced_at_utc");

		builder.Property(place => place.CreatedAtUtc)
			.HasColumnName("created_at_utc")
			.IsRequired();

		builder.Property(place => place.UpdatedAtUtc)
			.HasColumnName("updated_at_utc");

		builder.HasIndex(place => place.Category)
			.HasDatabaseName("ix_places_category");

		builder.HasIndex(place => new { place.Latitude, place.Longitude })
			.HasDatabaseName("ix_places_latitude_longitude");

		builder.HasIndex(place => place.IsActive)
			.HasDatabaseName("ix_places_is_active");

		builder.HasIndex(place => new { place.Source, place.ExternalId })
			.HasDatabaseName("ux_places_source_external_id")
			.IsUnique()
			.HasFilter("source IS NOT NULL AND external_id IS NOT NULL");
	}
}