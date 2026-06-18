using Domain.Places;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Places;

public sealed class RawOsmNodeConfiguration : IEntityTypeConfiguration<RawOsmNode>
{
	public void Configure(EntityTypeBuilder<RawOsmNode> builder)
	{
		builder.ToTable("raw_osm_nodes");

		builder.HasKey(n => n.Id);

		builder.Property(n => n.Id)
			.HasColumnName("id")
			.ValueGeneratedNever();

		builder.Property(n => n.Location)
			.HasColumnName("location")
			.HasColumnType("geometry(point, 4326)")
			.IsRequired();

		builder.Property(n => n.Name)
			.HasColumnName("name")
			.HasMaxLength(255);

		builder.Property(n => n.Tags)
			.HasColumnName("tags")
			.HasColumnType("jsonb");

		builder.HasIndex(n => n.Location)
			.HasDatabaseName("ix_raw_osm_nodes_location");
	}
}
