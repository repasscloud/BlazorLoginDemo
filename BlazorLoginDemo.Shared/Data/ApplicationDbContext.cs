// ApplicationDbContext.cs (overhauled)
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using BlazorLoginDemo.Shared.Models.Auth;                   // RefreshToken
using BlazorLoginDemo.Shared.Models.Kernel.Travel;          // TravelPolicy, Region, Continent, Country, etc.
using BlazorLoginDemo.Shared.Models.ExternalLib.Amadeus;    // AmadeusOAuthToken
using BlazorLoginDemo.Shared.Models.ExternalLib.Kernel.Flight;
using BlazorLoginDemo.Shared.Models.Kernel.SysVar;          // AvaSystemLog
using BlazorLoginDemo.Shared.Models.Kernel.Platform;        // OrganizationUnified, OrganizationDomainUnified
using BlazorLoginDemo.Shared.Models.Kernel.Billing;         // LicenseAgreementUnified
using BlazorLoginDemo.Shared.Models.Policies;               // ExpensePolicy
using BlazorLoginDemo.Shared.Models.User;
using BlazorLoginDemo.Shared.Models.DTOs;                   // AvaUserLoyaltyAccount (legacy shape retained)

namespace BlazorLoginDemo.Shared.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // ---------------------------
    // Core / Logs
    // ---------------------------
    public DbSet<AvaSystemLog> AvaSystemLogs => Set<AvaSystemLog>();

    // ---------------------------
    // Org / Tenancy (Unified)
    // ---------------------------
    public DbSet<OrganizationUnified> Organizations => Set<OrganizationUnified>();
    public DbSet<OrganizationDomainUnified> OrganizationDomains => Set<OrganizationDomainUnified>();

    // ---------------------------
    // Auth / Tokens
    // ---------------------------
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AmadeusOAuthToken> AmadeusOAuthTokens => Set<AmadeusOAuthToken>();

    // ---------------------------
    // Billing / Policies (Unified)
    // ---------------------------
    public DbSet<LicenseAgreementUnified> LicenseAgreements => Set<LicenseAgreementUnified>();
    public DbSet<ExpensePolicy> ExpensePolicies => Set<ExpensePolicy>();

    // ---------------------------
    // Travel / Geography
    // ---------------------------
    public DbSet<TravelPolicy> TravelPolicies => Set<TravelPolicy>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Continent> Continents => Set<Continent>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<TravelPolicyDisabledCountry> TravelPolicyDisabledCountries => Set<TravelPolicyDisabledCountry>();
    public DbSet<AirportInfo> AirportInfos => Set<AirportInfo>();

    // ---------------------------
    // Search / Results
    // ---------------------------
    public DbSet<FlightOfferSearchRequestDto> FlightOfferSearchRequestDtos => Set<FlightOfferSearchRequestDto>();
    public DbSet<FlightOfferSearchResultRecord> FlightOfferSearchResultRecords => Set<FlightOfferSearchResultRecord>();

    // ---------------------------
    // Ref data: Airlines / Loyalty
    // ---------------------------
    public DbSet<Airline> Airlines => Set<Airline>();
    public DbSet<LoyaltyProgram> LoyaltyPrograms => Set<LoyaltyProgram>();
    public DbSet<AvaUserLoyaltyAccount> UserLoyaltyAccounts => Set<AvaUserLoyaltyAccount>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ===========================
        // Core / Logs
        // ===========================
        builder.Entity<AvaSystemLog>(e =>
        {
            e.ToTable("ava_system_logs", "avasyslog");
            e.HasKey(x => x.Id);
        });

        // ===========================
        // ApplicationUser (overhauled)
        // ===========================
        builder.Entity<ApplicationUser>(e =>
        {
            // Org anchor -> OrganizationUnified
            e.HasOne(u => u.Organization)
                .WithMany() // just to have a backref target; doesn't create FK
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // self-ref manager
            e.HasOne(u => u.Manager)
                .WithMany(m => m!.DirectReports)
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(u => u.DisplayName).HasMaxLength(128);
            e.Property(u => u.Department).HasMaxLength(128);
            e.HasIndex(u => u.OrganizationId);

            // Map text[] arrays for airline codes (PostgreSQL)
            e.Property(u => u.IncludedAirlineCodes)
                .HasColumnType("text[]")
                .HasDefaultValueSql("'{}'::text[]")
                .IsRequired();

            e.Property(u => u.ExcludedAirlineCodes)
                .HasColumnType("text[]")
                .HasDefaultValueSql("'{}'::text[]")
                .IsRequired();
        }); // :contentReference[oaicite:10]{index=10}

        // RefreshToken -> ApplicationUser (shadow FK)
        builder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens", "ava");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Token).IsUnique();

            // Use a shadow FK named "ApplicationUserId" to avoid touching the token class now
            e.Property<string>("ApplicationUserId");
            e.HasOne<ApplicationUser>()
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey("ApplicationUserId")
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AmadeusOAuthToken>(t =>
        {
            t.ToTable("amadeus_oauth_tokens", "amadeus");
            t.HasKey(x => x.Id);
        });

        // ===========================
        // Organization (Unified)
        // ===========================
        builder.Entity<OrganizationUnified>(e =>
        {
            e.ToTable("organizations", "ava");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).HasMaxLength(128).IsRequired();

            e.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentOrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.CreatedAt)
                .HasColumnType("timestamptz")  // correct PG type
                .HasDefaultValueSql("now()")  // DB sets on INSERT (UTC under the hood)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(
                    Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
            
            e.HasIndex(x => new { x.Type, x.ParentOrganizationId });
        }); // :contentReference[oaicite:11]{index=11}

        builder.Entity<OrganizationDomainUnified>(e =>
        {
            e.ToTable("organization_domains", "ava");
            e.HasKey(x => x.Id);
            e.Property(x => x.Domain).HasMaxLength(190).IsRequired();
            e.HasIndex(x => x.Domain).IsUnique();

            e.HasOne(d => d.Organization)
                .WithMany(o => o.Domains)
                .HasForeignKey(d => d.OrganizationUnifiedId)
                .OnDelete(DeleteBehavior.Cascade);
        }); // :contentReference[oaicite:12]{index=12}

        // ===========================
        // LicenseAgreementUnified (+ owned subtypes)
        // ===========================
        builder.Entity<LicenseAgreementUnified>(e =>
        {
            e.ToTable("license_agreements", "ava");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(14);

            // Owning org (1:1 per org)
            e.HasOne(x => x.Organization)
             .WithOne(o => o.LicenseAgreement)
             .HasForeignKey<OrganizationUnified>(o => o.LicenseAgreementId)
             .OnDelete(DeleteBehavior.SetNull);

            // Issuer org (many agreements can be issued by one org)
            e.HasOne(x => x.CreatedByOrganization)
             .WithMany()
             .HasForeignKey(x => x.CreatedByOrganizationUnifiedId)
             .OnDelete(DeleteBehavior.Restrict);

            // Owned: DiscountA
            e.OwnsOne(x => x.DiscountA, d =>
            {
                d.Property(p => p.Amount).HasColumnName("DiscountA_Amount");
                d.Property(p => p.Scope).HasColumnName("DiscountA_Scope");
                d.Property(p => p.ExpiresOnUtc).HasColumnName("DiscountA_ExpiresOnUtc");
            });

            // Owned: DiscountB
            e.OwnsOne(x => x.DiscountB, d =>
            {
                d.Property(p => p.Percent).HasColumnName("DiscountB_Percent");
                d.Property(p => p.Scope).HasColumnName("DiscountB_Scope");
                d.Property(p => p.ExpiresOnUtc).HasColumnName("DiscountB_ExpiresOnUtc");
            });

            // Owned: LateFees
            e.OwnsOne(x => x.LateFees, lf =>
            {
                lf.Property(p => p.GracePeriodDays).HasColumnName("Late_GracePeriodDays");
                lf.Property(p => p.UseFixedAmount).HasColumnName("Late_UseFixedAmount");
                lf.Property(p => p.FixedAmount).HasColumnName("Late_FixedAmount");
                lf.Property(p => p.PercentOfInvoice).HasColumnName("Late_PercentOfInvoice");
                lf.Property(p => p.MaxLateFeeCap).HasColumnName("Late_MaxLateFeeCap");
                lf.Property(p => p.Terms).HasColumnName("Late_PaymentTerms");
            });
        }); // :contentReference[oaicite:13]{index=13}

        // ===========================
        // ExpensePolicy (new)
        // ===========================
        builder.Entity<ExpensePolicy>(e =>
        {
            e.ToTable("expense_policies", "ava");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.DefaultCurrency).HasMaxLength(3);

            e.HasOne(x => x.Organization)
             .WithMany(o => o.ExpensePolicies)
             .HasForeignKey(x => x.OrganizationUnifiedId)
             .OnDelete(DeleteBehavior.Cascade);
        }); // :contentReference[oaicite:14]{index=14}

        // ===========================
        // TravelPolicy (attach to Org via many-to-many for now)
        // ===========================
        builder.Entity<TravelPolicy>(e =>
        {
            e.ToTable("travel_policies", "ava");
            e.HasKey(x => x.Id);

            // NEW: owner org 1..* TravelPolicies
            e.HasOne(tp => tp.Organization)
            .WithMany(o => o.TravelPolicies)
            .HasForeignKey(tp => tp.OrganizationUnifiedId)
            .OnDelete(DeleteBehavior.Cascade);

            // existing many-to-many selectors
            e.HasMany(x => x.Regions)
            .WithMany()
            .UsingEntity(j => j.ToTable("travel_policy_regions", "ava"));

            e.HasMany(x => x.Continents)
            .WithMany()
            .UsingEntity(j => j.ToTable("travel_policy_continents", "ava"));

            e.HasMany(x => x.Countries)
            .WithMany()
            .UsingEntity(j => j.ToTable("travel_policy_countries", "ava"));

            // arrays
            e.Property(p => p.IncludedAirlineCodes)
            .HasColumnType("text[]").IsRequired()
            .HasDefaultValueSql("'{}'::text[]");
            e.Property(p => p.ExcludedAirlineCodes)
            .HasColumnType("text[]").IsRequired()
            .HasDefaultValueSql("'{}'::text[]");
        });

        // Disabled countries (your composite PK is fine)
        builder.Entity<TravelPolicyDisabledCountry>(e =>
        {
            e.ToTable("travel_policy_disabled_countries", "ava");
            e.HasKey(x => new { x.TravelPolicyId, x.CountryId });

            e.HasOne(x => x.TravelPolicy)
            .WithMany(tp => tp.DisabledCountries)
            .HasForeignKey(x => x.TravelPolicyId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
        });


        // Disabled country junction
        builder.Entity<TravelPolicyDisabledCountry>(e =>
        {
            e.ToTable("travel_policy_disabled_countries", "ava");
            e.HasKey(x => new { x.TravelPolicyId, x.CountryId });

            e.HasOne(x => x.TravelPolicy)
                .WithMany(tp => tp.DisabledCountries)
                .HasForeignKey(x => x.TravelPolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.CountryId);
        });

        // Geography hierarchy
        builder.Entity<Continent>(e =>
        {
            e.ToTable("continents", "ava");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Region)
                .WithMany(r => r.Continents)
                .HasForeignKey(x => x.RegionId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.RegionId);
        });

        builder.Entity<Country>(e =>
        {
            e.ToTable("countries", "ava");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Continent)
                .WithMany(ct => ct.Countries)
                .HasForeignKey(x => x.ContinentId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.ContinentId);
        });

        builder.Entity<Region>(e =>
        {
            e.ToTable("regions", "ava");
            e.HasKey(x => x.Id);
        });

        builder.Entity<AirportInfo>(e =>
        {
            e.ToTable("airport_infos", "ava");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Ident).IsUnique();
        });

        // ===========================
        // Airlines / Loyalty (ref + user data)
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

        // Rehome AvaUserLoyaltyAccount to ApplicationUser (shadow FK) and ignore legacy nav
        builder.Entity<AvaUserLoyaltyAccount>(e =>
        {
            e.ToTable("user_loyalty_accounts", "ava");
            e.HasKey(x => x.Id);

            // Ignore legacy nav to AvaUser so we can attach to ApplicationUser immediately
            e.Ignore(x => x.AvaUser);
            e.Ignore(x => x.AvaUserId);

            e.Property<string>("ApplicationUserId");
            e.HasOne<ApplicationUser>()
             .WithMany(u => u.LoyaltyAccounts)
             .HasForeignKey("ApplicationUserId")
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Program)
             .WithMany()
             .HasForeignKey(x => x.LoyaltyProgramId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex("ApplicationUserId", nameof(AvaUserLoyaltyAccount.LoyaltyProgramId)).IsUnique();
            e.Property(x => x.MembershipNumber).HasMaxLength(64).IsRequired();
        });

        // ===========================
        // Search Results
        // ===========================
        builder.Entity<FlightOfferSearchRequestDto>(e =>
        {
            e.ToTable("flight_offer_search_request_dtos", "ava");
            e.HasKey(x => x.Id);
        });

        builder.Entity<FlightOfferSearchResultRecord>(e =>
        {
            e.ToTable("flight_offer_search_result_records", "ava");
            e.HasKey(x => x.Id);

            e.Property(x => x.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            e.Property(x => x.AmadeusPayload).HasColumnType("jsonb");

            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => x.AvaUserId);
            e.HasIndex(x => x.ClientId);
            e.HasIndex(x => x.FlightOfferSearchRequestDtoId);
        });
    }
}
