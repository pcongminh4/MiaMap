using Domain.Places;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Places;

public sealed class RoadConfiguration : IEntityTypeConfiguration<Road>
{
	public void Configure(EntityTypeBuilder<Road> builder)
	{
		builder.ToTable("roads");

		builder.HasKey(r => r.Id);

		builder.Property(r => r.Id)
			.HasColumnName("id")
			.ValueGeneratedOnAdd();

		builder.Property(r => r.Source)
			.HasColumnName("source")
			.HasMaxLength(50);

		builder.Property(r => r.ExternalId)
			.HasColumnName("external_id")
			.HasMaxLength(100);

		builder.Property(r => r.Geometry)
			.HasColumnName("geometry")
			.HasColumnType("geometry(linestring, 4326)")
			.IsRequired();

		builder.Property(r => r.LengthMeters)
			.HasColumnName("length_meters")
			.IsRequired();

		builder.Property(r => r.StartNodeId)
			.HasColumnName("start_node_id")
			.IsRequired();

		builder.Property(r => r.EndNodeId)
			.HasColumnName("end_node_id")
			.IsRequired();

		builder.Property(r => r.IsOneWay)
			.HasColumnName("is_one_way")
			.IsRequired();

		builder.Property(r => r.RoadName)
			.HasColumnName("road_name")
			.HasMaxLength(255);

		builder.Property(r => r.RoadType)
			.HasColumnName("road_type")
			.HasMaxLength(50);

		builder.Property(r => r.MaxSpeedKmh)
			.HasColumnName("max_speed_kmh")
			.IsRequired();

		builder.Property(r => r.Weight)
			.HasColumnName("weight")
			.IsRequired();

		builder.Property(r => r.IsActive)
			.HasColumnName("is_active")
			.IsRequired();

		builder.Property(r => r.CreatedAtUtc)
			.HasColumnName("created_at_utc")
			.IsRequired();

		builder.Property(r => r.UpdatedAtUtc)
			.HasColumnName("updated_at_utc");

		builder.HasIndex(r => new { r.Source, r.ExternalId })
			.IsUnique();

		builder.HasOne(r => r.StartNode)
			.WithMany(n => n.OutgoingRoads)
			.HasForeignKey(r => r.StartNodeId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(r => r.EndNode)
			.WithMany(n => n.IncomingRoads)
			.HasForeignKey(r => r.EndNodeId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}