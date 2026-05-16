using Domain.Places;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Places;

public sealed class NodeConfiguration : IEntityTypeConfiguration<Node>
{
	public void Configure(EntityTypeBuilder<Node> builder)
	{
		builder.ToTable("nodes");

		builder.HasKey(n => n.Id);

		builder.Property(n => n.Id)
			.HasColumnName("id")
			.ValueGeneratedOnAdd();

		builder.Property(n => n.Source)
			.HasColumnName("source")
			.HasMaxLength(50);

		builder.Property(n => n.ExternalId)
			.HasColumnName("external_id")
			.HasMaxLength(100);

		builder.Property(n => n.Location)
			.HasColumnName("location")
			.HasColumnType("geometry(point, 4326)")
			.IsRequired();

		builder.Property(n => n.Name)
			.HasColumnName("name")
			.HasMaxLength(255);

		builder.Property(n => n.IsActive)
			.HasColumnName("is_active")
			.IsRequired();

		builder.Property(n => n.CreatedAtUtc)
			.HasColumnName("created_at_utc")
			.IsRequired();

		builder.Property(n => n.UpdatedAtUtc)
			.HasColumnName("updated_at_utc");

		builder.HasIndex(n => new { n.Source, n.ExternalId })
			.IsUnique();
	}
}