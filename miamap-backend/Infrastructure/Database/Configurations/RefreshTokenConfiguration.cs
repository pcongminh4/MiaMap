using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
	public void Configure(EntityTypeBuilder<RefreshToken> builder)
	{
		builder.ToTable("refresh_tokens");

		builder.HasKey(rt => rt.Id);

		builder.Property(rt => rt.Id)
			.HasColumnName("id")
			.ValueGeneratedOnAdd();

		builder.Property(rt => rt.UserId)
			.HasColumnName("user_id")
			.IsRequired();

		builder.Property(rt => rt.Token)
			.HasColumnName("token")
			.HasMaxLength(200)
			.IsRequired();

		builder.HasIndex(rt => rt.Token)
			.IsUnique();

		builder.Property(rt => rt.ExpiresAtUtc)
			.HasColumnName("expires_at_utc")
			.IsRequired();

		builder.Property(rt => rt.IsRevoked)
			.HasColumnName("is_revoked")
			.HasDefaultValue(false)
			.IsRequired();

		builder.Property(rt => rt.CreatedAtUtc)
			.HasColumnName("created_at_utc")
			.IsRequired();

		builder.HasOne<User>()
			.WithMany()
			.HasForeignKey(rt => rt.UserId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
