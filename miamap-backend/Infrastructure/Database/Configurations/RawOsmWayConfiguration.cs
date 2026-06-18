using Domain.Places;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Places;

public sealed class RawOsmWayConfiguration : IEntityTypeConfiguration<RawOsmWay>
{
	public void Configure(EntityTypeBuilder<RawOsmWay> builder)
	{
		builder.ToTable("raw_osm_ways");

		builder.HasKey(w => w.Id);

		builder.Property(w => w.Id)
			.HasColumnName("id")
			.ValueGeneratedNever();

		builder.Property(w => w.NodeIds)
			.HasColumnName("node_ids")
			.IsRequired();

		builder.Property(w => w.Tags)
			.HasColumnName("tags")
			.HasColumnType("jsonb");

		builder.Property(w => w.Highway)
			.HasColumnName("highway")
			.HasMaxLength(50);

		builder.Property(w => w.Name)
			.HasColumnName("name")
			.HasMaxLength(255);

		builder.Property(w => w.Oneway)
			.HasColumnName("oneway")
			.HasMaxLength(50);

		builder.Property(w => w.Maxspeed)
			.HasColumnName("maxspeed")
			.HasMaxLength(50);
	}
}
