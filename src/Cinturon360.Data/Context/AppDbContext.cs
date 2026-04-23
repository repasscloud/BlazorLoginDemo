using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace Cinturon360.Data.Context;

/// <summary>
/// Single EF Core DbContext for Cinturon360.
/// All entity configurations are loaded from the Configurations/ folder.
/// OpenIddict tables are registered via UseOpenIddict().
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Register all entity type configurations from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // OpenIddict tables
        builder.UseOpenIddict();
    }
}
