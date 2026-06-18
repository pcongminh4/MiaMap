using Application.Common.Abstractions.Data;
using Domain.Places;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
	public ApplicationDbContext(
		DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	public DbSet<User> Users => Set<User>();

	public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

	public DbSet<Place> Places => Set<Place>();

	public DbSet<Report> Reports => Set<Report>();

	public DbSet<ReportVote> ReportVotes => Set<ReportVote>();

	public DbSet<Node> Nodes => Set<Node>();

	public DbSet<Road> Roads => Set<Road>();

	public DbSet<RawOsmNode> RawOsmNodes => Set<RawOsmNode>();

	public DbSet<RawOsmWay> RawOsmWays => Set<RawOsmWay>();

	public DbSet<RawOsmPlace> RawOsmPlaces => Set<RawOsmPlace>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}

	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		return await base.SaveChangesAsync(cancellationToken);
	}
}

