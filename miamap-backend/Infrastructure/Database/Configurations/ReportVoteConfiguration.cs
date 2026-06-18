using Domain.Places;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Places;

public sealed class ReportVoteConfiguration : IEntityTypeConfiguration<ReportVote>
{
    public void Configure(EntityTypeBuilder<ReportVote> builder)
    {
        builder.ToTable("report_votes");

        builder.HasKey(v => new { v.ReportId, v.UserId });

        builder.Property(v => v.ReportId)
            .HasColumnName("report_id");

        builder.Property(v => v.UserId)
            .HasColumnName("user_id");

        builder.Property(v => v.IsUpvote)
            .HasColumnName("is_upvote")
            .IsRequired();

        builder.Property(v => v.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasOne<Report>()
            .WithMany()
            .HasForeignKey(v => v.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
