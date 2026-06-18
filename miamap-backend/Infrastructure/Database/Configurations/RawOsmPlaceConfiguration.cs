using Domain.Places;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Places;

public sealed class RawOsmPlaceConfiguration : IEntityTypeConfiguration<RawOsmPlace>
{
	public void Configure(EntityTypeBuilder<RawOsmPlace> builder)
	{
		builder.ToTable("raw_osm_places");

		builder.HasKey(p => p.Id);

		builder.Property(p => p.Id)
			.HasColumnName("id")
			.ValueGeneratedNever();

		builder.Property(p => p.Location)
			.HasColumnName("location")
			.HasColumnType("geometry(point, 4326)")
			.IsRequired();

		builder.Property(p => p.Name)
			.HasColumnName("name")
			.HasMaxLength(255)
			.IsRequired();

		builder.Property(p => p.Category)
			.HasColumnName("category")
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(p => p.Address)
			.HasColumnName("address")
			.HasMaxLength(500);

		builder.Property(p => p.Tags)
			.HasColumnName("tags")
			.HasColumnType("jsonb");

		builder.HasIndex(p => p.Location)
			.HasDatabaseName("ix_raw_osm_places_location");
	}
}
