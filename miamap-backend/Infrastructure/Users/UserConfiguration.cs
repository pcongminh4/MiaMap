using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("users");

		builder.HasKey(user => user.Id);

		builder.Property(user => user.Id)
			.HasColumnName("id")
			.ValueGeneratedOnAdd();

		builder.Property(user => user.FirstName)
			.HasColumnName("first_name")
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(user => user.LastName)
			.HasColumnName("last_name")
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(user => user.Email)
			.HasColumnName("email")
			.HasMaxLength(255)
			.IsRequired();

		builder.HasIndex(user => user.Email)
			.IsUnique();

		builder.Property(user => user.PasswordHash)
			.HasColumnName("password_hash")
			.HasMaxLength(512)
			.IsRequired();

		builder.Property(user => user.CreatedAtUtc)
			.HasColumnName("created_at_utc")
			.IsRequired();

		builder.Property(user => user.UpdatedAtUtc)
			.HasColumnName("updated_at_utc");
	}
}
