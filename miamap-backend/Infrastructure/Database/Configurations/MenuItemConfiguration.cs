using Domain.Places;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Places;

public sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
	public void Configure(EntityTypeBuilder<MenuItem> builder)
	{
		builder.ToTable("menu_items");

		builder.HasKey(item => item.Id);

		builder.Property(item => item.Id)
			.HasColumnName("id")
			.ValueGeneratedOnAdd();

		builder.Property(item => item.PlaceId)
			.HasColumnName("place_id")
			.IsRequired();

		builder.Property(item => item.Name)
			.HasColumnName("name")
			.HasMaxLength(200)
			.IsRequired();

		builder.Property(item => item.Description)
			.HasColumnName("description")
			.HasMaxLength(500);

		builder.Property(item => item.Price)
			.HasColumnName("price")
			.HasColumnType("numeric(18,2)")
			.IsRequired();

		builder.Property(item => item.CreatedAtUtc)
			.HasColumnName("created_at_utc")
			.IsRequired();

		builder.HasOne(item => item.Place)
			.WithMany()
			.HasForeignKey(item => item.PlaceId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasIndex(item => item.PlaceId)
			.HasDatabaseName("ix_menu_items_place_id");

		builder.HasIndex(item => item.Name)
			.HasDatabaseName("ix_menu_items_name");
	}
}
