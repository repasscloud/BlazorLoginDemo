using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using BlazorLoginDemo.Shared.Models.User;
using BlazorLoginDemo.Shared.Models.Auth;
using BlazorLoginDemo.Shared.Models.Kernel.Billing;
using BlazorLoginDemo.Shared.Models.Kernel.User;
using BlazorLoginDemo.Shared.Models.Kernel.Travel;
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;
using BlazorLoginDemo.Shared.Models.Kernel.SysVar;
using BlazorLoginDemo.Shared.Models.DTOs;
using BlazorLoginDemo.Shared.Models.ExternalLib.Kernel.Flight;
using System.Security.Cryptography.X509Certificates;
using BlazorLoginDemo.Shared.Models.Kernel.Platform;

namespace BlazorLoginDemo.Shared.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // ---------------------------
    // Core / Groups
    // ---------------------------
    public DbSet<AvaSystemLog> AvaSystemLogs => Set<AvaSystemLog>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationDomain> OrganizationDomains => Set<OrganizationDomain>();
    
    // ---------------------------
    // Auth / Tokens
    // ---------------------------
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AmadeusOAuthToken> AmadeusOAuthTokens => Set<AmadeusOAuthToken>();

    // ---------------------------
    // Ava (Users, Clients, Licensing)
    // ---------------------------
    public DbSet<AvaUser> AvaUsers => Set<AvaUser>();
    public DbSet<AvaUserSysPreference> AvaUserSysPreferences => Set<AvaUserSysPreference>();
    public DbSet<AvaClient> AvaClients => Set<AvaClient>();
    public DbSet<AvaClientLicense> AvaClientLicenses => Set<AvaClientLicense>();
    public DbSet<LicenseAgreement> LicenseAgreements => Set<LicenseAgreement>();

    // ---------------------------
    // Ava Travel: Airlines / Loyalty
    // ---------------------------
    public DbSet<Airline> Airlines => Set<Airline>();
    public DbSet<LoyaltyProgram> LoyaltyPrograms => Set<LoyaltyProgram>();
    public DbSet<AvaUserLoyaltyAccount> UserLoyaltyAccounts => Set<AvaUserLoyaltyAccount>();

    // ---------------------------
    // Amadeus: Internal
    // ---------------------------
    public DbSet<FlightOfferSearchRequestDto> FlightOfferSearchRequestDtos => Set<FlightOfferSearchRequestDto>();

    // ---------------------------
    // Search Results
    // ---------------------------
    public DbSet<FlightOfferSearchResultRecord> FlightOfferSearchResultRecords => Set<FlightOfferSearchResultRecord>();
    
    // ---------------------------
    // Finance
    // ---------------------------
    public DbSet<LateFeeConfig> LateFeeConfigs => Set<LateFeeConfig>();

    // ---------------------------
    // Travel Policy / Geography
    // ---------------------------
    public DbSet<TravelPolicy> TravelPolicies => Set<TravelPolicy>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Continent> Continents => Set<Continent>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<TravelPolicyDisabledCountry> TravelPolicyDisabledCountries => Set<TravelPolicyDisabledCountry>();
    public DbSet<AirportInfo> AirportInfos => Set<AirportInfo>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ===========================
        // Core / Groups
        // ===========================
        builder.Entity<AvaSystemLog>(e =>
        {
            e.ToTable("ava_system_logs", "avasyslog");
            e.HasKey(x => x.Id);
        });

        builder.Entity<Organization>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentOrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Type, x.ParentOrganizationId });
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
        });

        builder.Entity<OrganizationDomain>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Domain).IsUnique();
            e.Property(x => x.Domain).HasMaxLength(190).IsRequired();
        });

        // ===========================
        // Auth / ApplicationUser
        // ===========================
        builder.Entity<ApplicationUser>(e =>
        {
            e.HasOne(u => u.Organization)
                .WithMany()
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            
            e.Property(u => u.DisplayName).HasMaxLength(128);
            e.Property(u => u.Department).HasMaxLength(128);
        });

        builder.Entity<RefreshToken>(b =>
        {
            b.HasIndex(x => x.Token).IsUnique();

            b.HasOne(x => x.AvaUser)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(x => x.AvaUserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AmadeusOAuthToken>(t =>
        {
            t.ToTable("amadeus_oauth_tokens", "amadeus");
            t.HasKey(x => x.Id);
        });

        // ===========================
        // Ava Users & Sys Preferences
        // ===========================
        builder.Entity<AvaUserSysPreference>(e =>
        {
            e.ToTable("ava_user_sys_preferences", "ava");
            e.HasKey(x => x.Id);

            e.Property(x => x.FirstName).IsRequired();
            e.Property(x => x.LastName).IsRequired();
            e.Property(x => x.Email).IsRequired();

            // enforce uniqueness of Email inside prefs table
            e.HasIndex(x => x.Email).IsUnique();

            e.Property(p => p.IncludedAirlineCodes)
             .HasColumnType("text[]")
             .IsRequired()
             .HasDefaultValueSql("'{}'::text[]");

            e.Property(p => p.ExcludedAirlineCodes)
             .HasColumnType("text[]")
             .IsRequired()
             .HasDefaultValueSql("'{}'::text[]");
        });

        builder.Entity<AvaUser>(e =>
        {
            e.ToTable("ava_users", "ava");

            e.HasKey(x => x.Id);
            e.Property(x => x.AspNetUsersId).IsRequired();

            // enforce email uniqueness
            e.HasIndex(x => x.Email).IsUnique();

            // 1:1 AvaUser (FK) -> ApplicationUser (PK)
            e.HasOne<ApplicationUser>()
             .WithOne(u => u.Profile)
             .HasForeignKey<AvaUser>(x => x.AspNetUsersId)
             .OnDelete(DeleteBehavior.Cascade);

            // Optional 1:1 AvaUser ↔ AvaUserSysPreference (nullable FK on AvaUser)
            e.Property(x => x.AvaUserSysPreferenceId).IsRequired(false);

            e.HasOne<AvaUserSysPreference>()
             .WithOne()
             .HasForeignKey<AvaUser>(x => x.AvaUserSysPreferenceId)
             .HasPrincipalKey<AvaUserSysPreference>(p => p.Id)
             .OnDelete(DeleteBehavior.SetNull);

            // Unique index over nullable FK (filter for SQL Server to allow many NULLs)
            e.HasIndex(x => x.AvaUserSysPreferenceId).IsUnique()
#if SQLSERVER
                .HasFilter("[AvaUserSysPreferenceId] IS NOT NULL")
#endif
                ;
        });

        // ===========================
        // Ava Clients & Licensing
        // ===========================
        builder.Entity<LicenseAgreement>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(14);

            e.Property(x => x.AvaClientId).IsRequired();
            e.HasIndex(x => x.AvaClientId).IsUnique();
        });

        builder.Entity<AvaClient>(e =>
        {
            e.Property(x => x.LicenseAgreementId).HasMaxLength(14);
            e.HasIndex(x => x.LicenseAgreementId).IsUnique();

            e.HasOne<LicenseAgreement>()
             .WithOne()
             .HasForeignKey<AvaClient>(c => c.LicenseAgreementId)
             .OnDelete(DeleteBehavior.SetNull)
             .HasConstraintName("FK_AvaClient_LicenseAgreement");

            // DefaultTravelPolicy (1 AvaClient -> optional 1 TravelPolicy as default)
            e.HasOne(ac => ac.DefaultTravelPolicy)
             .WithMany()
             .HasForeignKey(ac => ac.DefaultTravelPolicyId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // AvaClient (1) -> (many) TravelPolicies
        builder.Entity<TravelPolicy>()
            .HasOne(tp => tp.AvaClient)
            .WithMany(ac => ac.TravelPolicies)
            .HasForeignKey(tp => tp.AvaClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===========================
        // Amadeus: Internal / External
        // ===========================

        builder.Entity<FlightOfferSearchRequestDto>(e =>
        {
            e.HasKey(x => x.Id);
        });

        // ===========================
        // Search Results
        // ===========================

        builder.Entity<FlightOfferSearchResultRecord>(e =>
        {
            e.ToTable("flight_offer_search_result_records", "ava");
            e.HasKey(x => x.Id);

            e.Property(x => x.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            e.Property(x => x.AmadeusPayload)
                .HasColumnType("jsonb");

            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => x.AvaUserId);
            e.HasIndex(x => x.ClientId);
            e.HasIndex(x => x.FlightOfferSearchRequestDtoId);
        });

        // ===========================
        // Travel Policy / Geography
        // ===========================
        builder.Entity<TravelPolicy>()
            .HasMany(tp => tp.Regions)
            .WithMany();

        builder.Entity<TravelPolicy>()
            .HasMany(tp => tp.Continents)
            .WithMany();

        builder.Entity<TravelPolicy>()
            .HasMany(tp => tp.Countries)
            .WithMany();

        // Explicit composite key for disabled countries (junction)
        builder.Entity<TravelPolicyDisabledCountry>()
            .HasKey(tpdc => new { tpdc.TravelPolicyId, tpdc.CountryId });

        // Region (1) -> (many) Continents
        builder.Entity<Continent>()
            .HasOne(c => c.Region)
            .WithMany(r => r.Continents)
            .HasForeignKey(c => c.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Continent (1) -> (many) Countries
        builder.Entity<Country>()
            .HasOne(c => c.Continent)
            .WithMany(ct => ct.Countries)
            .HasForeignKey(c => c.ContinentId)
            .OnDelete(DeleteBehavior.Restrict);

        // TravelPolicy arrays (PostgreSQL)
        builder.Entity<TravelPolicy>(e =>
        {
            e.Property(p => p.IncludedAirlineCodes)
             .HasColumnType("text[]")
             .IsRequired()
             .HasDefaultValueSql("'{}'::text[]");

            e.Property(p => p.ExcludedAirlineCodes)
             .HasColumnType("text[]")
             .IsRequired()
             .HasDefaultValueSql("'{}'::text[]");
        });

        // AirportInfo
        builder.Entity<AirportInfo>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Ident).IsUnique();
        });

        // ===========================
        // Airlines / Loyalty (ref data)
        // ===========================
        builder.Entity<Airline>(e =>
        {
            e.ToTable("airlines", "ava");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Iata).IsUnique();
            e.Property(x => x.Iata).HasMaxLength(3).IsRequired();
            e.Property(x => x.Icao).HasMaxLength(4);
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
        });

        builder.Entity<LoyaltyProgram>(e =>
        {
            e.ToTable("loyalty_programs", "ava");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(32).IsRequired();
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();

            e.HasOne(x => x.Airline)
             .WithMany(a => a.Programs)
             .HasForeignKey(x => x.AirlineId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ===========================
        // User Loyalty Accounts (user data)
        // ===========================
        builder.Entity<AvaUserLoyaltyAccount>(e =>
        {
            e.ToTable("user_loyalty_accounts", "ava");
            e.HasKey(x => x.Id);

            // One user can have many accounts (multi-program support).
            // Use WithMany() to avoid requiring a nav on AvaUser.
            e.HasOne(x => x.AvaUser)
             .WithMany()
             .HasForeignKey(x => x.AvaUserId)
             .OnDelete(DeleteBehavior.Cascade);

            // Each account belongs to exactly one LoyaltyProgram.
            e.HasOne(x => x.Program)
             .WithMany() // no back-collection on LoyaltyProgram required
             .HasForeignKey(x => x.LoyaltyProgramId)
             .OnDelete(DeleteBehavior.Restrict);

            // Prevent duplicates of the same program per user.
            e.HasIndex(x => new { x.AvaUserId, x.LoyaltyProgramId }).IsUnique();

            e.Property(x => x.MembershipNumber).HasMaxLength(64).IsRequired();
        });
    }
}
