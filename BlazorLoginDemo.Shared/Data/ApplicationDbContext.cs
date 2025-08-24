using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlazorLoginDemo.Shared.Models.User;
using BlazorLoginDemo.Shared.Models.Auth;


namespace BlazorLoginDemo.Shared.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupDomain> GroupDomains => Set<GroupDomain>();

    // API
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Ava
    public DbSet<AvaUser> AvaUsers => Set<AvaUser>();
    public DbSet<AvaClient> AvaClients => Set<AvaClient>();

    // Travel Policy
    public DbSet<TravelPolicy> TravelPolicies => Set<TravelPolicy>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Continent> Continents => Set<Continent>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<TravelPolicyDisabledCountry> TravelPolicyDisabledCountries => Set<TravelPolicyDisabledCountry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Group>(e =>
        {
            e.HasIndex(x => x.Name).IsUnique();
            e.HasMany(x => x.Domains)
             .WithOne(x => x.Group)
             .HasForeignKey(x => x.GroupId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GroupDomain>(e =>
        {
            e.HasIndex(x => x.Domain).IsUnique();
        });

        builder.Entity<ApplicationUser>(e =>
        {
            e.HasOne(u => u.Group)
             .WithMany()
             .HasForeignKey(u => u.GroupId);

            e.Property(u => u.DisplayName).HasMaxLength(128);
            e.Property(u => u.Department).HasMaxLength(128);
        });

        builder.Entity<AvaUser>(e =>
        {
            // put the table where you want
            e.ToTable("ava_users", "ava");

            e.HasKey(x => x.Id);
            e.Property(x => x.AspNetUsersId).IsRequired();

            // enforce email uniqueness (at db level)
            e.HasIndex(x => x.Email).IsUnique();

            // 1:1 AvaUser (FK) -> ApplicationUser (PK)
            e.HasOne<ApplicationUser>()  // principal
                .WithOne(u => u.Profile)  // navigation on principal
                .HasForeignKey<AvaUser>(x => x.AspNetUsersId)
                .OnDelete(DeleteBehavior.Cascade);  // delete profile when user is deleted
        });

        // Configure the DefaultTravelPolicy relationship for AvaClient.
        builder.Entity<AvaClient>()
            .HasOne(ac => ac.DefaultTravelPolicy)
            .WithMany() // No navigation property on TravelPolicy for default.
            .HasForeignKey(ac => ac.DefaultTravelPolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        // configure the one-to-many relationship between AvaClient and TravelPolicy
        builder.Entity<TravelPolicy>()
            .HasOne(tp => tp.AvaClient)
            .WithMany(ac => ac.TravelPolicies)
            .HasForeignKey(tp => tp.AvaClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure many-to-many relationships for TravelPolicy.
        builder.Entity<TravelPolicy>()
            .HasMany(tp => tp.Regions)
            .WithMany();

        builder.Entity<TravelPolicy>()
            .HasMany(tp => tp.Continents)
            .WithMany();

        builder.Entity<TravelPolicy>()
            .HasMany(tp => tp.Countries)
            .WithMany();

        // Explicit composite key configuration for disabled countries.
        builder.Entity<TravelPolicyDisabledCountry>()
            .HasKey(tpdc => new { tpdc.TravelPolicyId, tpdc.CountryId });

        // Configure one-to-many for Region and Continent.
        builder.Entity<Continent>()
            .HasOne(c => c.Region)
            .WithMany(r => r.Continents)
            .HasForeignKey(c => c.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure one-to-many for Continent and Country.
        builder.Entity<Country>()
            .HasOne(c => c.Continent)
            .WithMany(ct => ct.Countries)
            .HasForeignKey(c => c.ContinentId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<RefreshToken>(b =>
        {
            b.HasIndex(x => x.Token).IsUnique();
            b.HasOne(x => x.AvaUser)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(x => x.AvaUserId)
            .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
