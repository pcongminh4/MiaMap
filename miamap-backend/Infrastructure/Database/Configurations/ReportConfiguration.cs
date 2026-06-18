using Domain.Places;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Places;

public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(r => r.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(r => r.ReportType)
            .HasColumnName("report_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.SubType)
            .HasColumnName("sub_type")
            .HasMaxLength(50);

        builder.Property(r => r.Location)
            .HasColumnName("location")
            .HasColumnType("geometry(point, 4326)")
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(r => r.Upvotes)
            .HasColumnName("upvotes")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(r => r.Downvotes)
            .HasColumnName("downvotes")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(r => r.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(r => r.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .IsRequired();

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Votes)
            .WithOne()
            .HasForeignKey(v => v.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        var navigation = builder.Metadata.FindNavigation(nameof(Report.Votes));
        navigation?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(r => r.Location)
            .HasDatabaseName("ix_reports_location");
    }
}
