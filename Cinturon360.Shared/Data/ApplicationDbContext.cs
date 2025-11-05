// ApplicationDbContext.cs (overhauled)
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Cinturon360.Shared.Models.Auth;                   // RefreshToken
using Cinturon360.Shared.Models.Kernel.Travel;          // TravelPolicy, Region, Continent, Country, etc.
using Cinturon360.Shared.Models.ExternalLib.Amadeus;    // AmadeusOAuthToken
using Cinturon360.Shared.Models.ExternalLib.Kernel.Flight;
using Cinturon360.Shared.Models.Kernel.SysVar;          // AvaSystemLog
using Cinturon360.Shared.Models.Kernel.Platform;        // OrganizationUnified, OrganizationDomainUnified
using Cinturon360.Shared.Models.Kernel.Billing;         // LicenseAgreementUnified
using Cinturon360.Shared.Models.Policies;               // ExpensePolicy
using Cinturon360.Shared.Models.User;
using Cinturon360.Shared.Models.DTOs;
using Cinturon360.Shared.Models.Kernel.FX;
using Cinturon360.Shared.Models.Static.Travel;                   // AvaUserLoyaltyAccount (legacy shape retained)

namespace Cinturon360.Shared.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // ---------------------------
    // Core / Logs
    // ---------------------------
    public DbSet<AvaSystemLog> AvaSystemLogs => Set<AvaSystemLog>();
    public DbSet<ErrorCodeUnified> ErrorCodes => Set<ErrorCodeUnified>();
    public DbSet<ExchangeRateSnapshot> ExchangeRateSnapshots => Set<ExchangeRateSnapshot>();

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
    public DbSet<Discount> Discounts => Set<Discount>();

    // ---------------------------
    // Travel / Geography
    // ---------------------------
    public DbSet<TravelPolicy> TravelPolicies => Set<TravelPolicy>();
    public DbSet<EphemeralTravelPolicy> EphemeralTravelPolicies => Set<EphemeralTravelPolicy>();
    public DbSet<FlightViewOption> FlightViewOptions => Set<FlightViewOption>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Continent> Continents => Set<Continent>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<AirportInfo> AirportInfos => Set<AirportInfo>();

    // ---------------------------
    // Search / Results
    // ---------------------------
    public DbSet<FlightOfferSearchRequestDto> FlightOfferSearchRequestDtos => Set<FlightOfferSearchRequestDto>();
    public DbSet<FlightOfferSearchResultRecord> FlightOfferSearchResultRecords => Set<FlightOfferSearchResultRecord>();

    // ---------------------------
    // Travel Bookings
    // ---------------------------
    public DbSet<TravelQuote> TravelQuotes => Set<TravelQuote>();
    public DbSet<TravelQuoteUser> TravelQuoteUsers => Set<TravelQuoteUser>();

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

        builder.Entity<ErrorCodeUnified>(e =>
        {
            e.ToTable("ava_error_codes", "ava");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ErrorCode)
                .IsUnique();

            e.Property(x => x.ErrorCode)
                .IsRequired()
                .HasMaxLength(50);
            e.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);
            e.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(2000);
            e.Property(x => x.Resolution)
                .HasMaxLength(2000);
            e.Property(x => x.ContactSupportLink)
                .HasMaxLength(500);

            e.Property(x => x.IsClientFacing)
                .HasDefaultValue(true);
            e.Property(x => x.IsInternalFacing)
                .HasDefaultValue(false);

            e.Property(x => x.CreatedOnUtc)
                .IsRequired()
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")          // DB sets UTC automatically
                .ValueGeneratedOnAdd();

            e.Property(x => x.CreatedOnUtc)
                .Metadata.SetAfterSaveBehavior(
                    Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        });

        // ===========================
        // FX snapshots
        // ===========================
        builder.Entity<ExchangeRateSnapshot>(e =>
        {
            e.ToTable("fx_rate_snapshots", "ava");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id");

            e.Property(x => x.BaseCode)
                .HasColumnName("base_code")
                .HasMaxLength(3)
                .IsRequired();

            e.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("timezone('utc', now())")
                .ValueGeneratedOnAdd();

            // prevent updates to created timestamp
            e.Property(x => x.CreatedAtUtc)
                .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

            // jsonb for rate map
            e.Property(x => x.Rates)
                .HasColumnName("rates_json")
                .HasColumnType("jsonb");

            e.Property(x => x.ProviderLastUpdateUtc)
                .HasColumnName("provider_last_update_utc")
                .HasColumnType("timestamptz");

            e.Property(x => x.ProviderNextUpdateUtc)
                .HasColumnName("provider_next_update_utc")
                .HasColumnType("timestamptz");

            e.Property(x => x.ProviderResult)
                .HasColumnName("provider_result")
                .HasMaxLength(32);

            // latest-by-base lookup
            e.HasIndex(x => new { x.BaseCode, x.CreatedAtUtc })
                .HasDatabaseName("ix_fx_base_created");
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
            e.HasIndex(x => x.LicenseAgreementId).IsUnique();

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

            e.HasIndex(x => x.OrganizationUnifiedId).IsUnique();

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
        // Billing
        // ===========================
        builder.Entity<Discount>(e =>
        {
            e.ToTable("subscription_discounts", "ava");
            e.HasKey(x => x.Id);
            e.Property(x => x.DiscountCode)
                .IsRequired()
                .HasMaxLength(30);

            e.HasIndex(x => x.DiscountCode)
                .IsUnique();
        });


        // ===========================
        // TravelPolicy (attach to Org via many-to-many for now)
        // ===========================
        builder.Entity<TravelPolicy>(e =>
        {
            e.ToTable("travel_policies", "ava");
            e.HasKey(x => x.Id);

            // Owner org 1..* TravelPolicies
            e.HasOne(tp => tp.Organization)
                .WithMany(o => o.TravelPolicies) // ensure OrganizationUnified has ICollection<TravelPolicy> TravelPolicies
                .HasForeignKey(tp => tp.OrganizationUnifiedId)
                .OnDelete(DeleteBehavior.Cascade);

            // issue #46
            e.Property(p => p.RegionIds)
                .HasColumnName("region_ids")
                .HasColumnType("integer[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::integer[]");

            e.Property(p => p.ContinentIds)
                .HasColumnName("continent_ids")
                .HasColumnType("integer[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::integer[]");

            e.Property(p => p.CountryIds)
                .HasColumnName("country_ids")
                .HasColumnType("integer[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::integer[]");

            e.Property(p => p.DisabledCountryIds)
                .HasColumnName("disabled_country_ids")
                .HasColumnType("integer[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::integer[]");

            // Arrays (PostgreSQL text[])
            e.Property(p => p.IncludedAirlineCodes)
                .HasColumnType("text[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::text[]");

            e.Property(p => p.ExcludedAirlineCodes)
                .HasColumnType("text[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::text[]");

            e.Property(p => p.IncludedHotelChains)
                .HasColumnType("text[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::text[]");

            e.Property(p => p.ExcludedHotelChains)
                .HasColumnType("text[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::text[]");

            e.Property(p => p.IncludedRailOperators)
                .HasColumnType("text[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::text[]");

            e.Property(p => p.ExcludedRailOperators)
                .HasColumnType("text[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::text[]");

            // Hire car UI caps (new)
            e.Property(p => p.DefaultCarClass).HasMaxLength(64);
            e.Property(p => p.MaxCarClass).HasMaxLength(64);
            e.Property(p => p.MaxCarDailyRate).HasColumnType("numeric(14,2)");

            // Other money caps already annotated; keep numeric(14,2) if you want to enforce here as well:
            // e.Property(p => p.MaxHotelNightlyRate).HasColumnType("numeric(14,2)");
            // e.Property(p => p.MaxTrainPrice).HasColumnType("numeric(14,2)");
            // e.Property(p => p.L1ApprovalAmount).HasColumnType("numeric(14,2)");
            // e.Property(p => p.L2ApprovalAmount).HasColumnType("numeric(14,2)");
            // e.Property(p => p.L3ApprovalAmount).HasColumnType("numeric(14,2)");

            // Policy window (new)
            e.Property(p => p.EffectiveFromUtc).HasColumnType("timestamptz");
            e.Property(p => p.ExpiresOnUtc).HasColumnType("timestamptz");

            // Auditing (new)
            e.Property(p => p.CreatedAtUtc)
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("timezone('utc', now())")
                .ValueGeneratedOnAdd();

            // Prevent updates to CreatedAtUtc after insert
            e.Property(p => p.CreatedAtUtc)
                .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

            e.Property(p => p.LastUpdatedUtc)
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("timezone('utc', now())");
        });

        // ===========================
        // Ephemeral Travel Policy (new)
        // ===========================
        builder.Entity<EphemeralTravelPolicy>(e =>
        {
            e.ToTable("ephemeral_travel_policies", "ava");
            e.HasKey(x => x.Id);
            e.HasKey(x => x.OrganizationUnifiedId);

            // issue #46
            e.Property(p => p.RegionIds)
                .HasColumnName("region_ids")
                .HasColumnType("integer[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::integer[]");

            e.Property(p => p.ContinentIds)
                .HasColumnName("continent_ids")
                .HasColumnType("integer[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::integer[]");

            e.Property(p => p.CountryIds)
                .HasColumnName("country_ids")
                .HasColumnType("integer[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::integer[]");

            e.Property(p => p.DisabledCountryIds)
                .HasColumnName("disabled_country_ids")
                .HasColumnType("integer[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::integer[]");

            // Arrays (PostgreSQL text[])
            e.Property(p => p.IncludedAirlineCodes)
                .HasColumnType("text[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::text[]");

            e.Property(p => p.ExcludedAirlineCodes)
                .HasColumnType("text[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::text[]");

            e.Property(p => p.IncludedHotelChains)
                .HasColumnType("text[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::text[]");

            e.Property(p => p.ExcludedHotelChains)
                .HasColumnType("text[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::text[]");

            e.Property(p => p.IncludedRailOperators)
                .HasColumnType("text[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::text[]");

            e.Property(p => p.ExcludedRailOperators)
                .HasColumnType("text[]")
                .IsRequired()
                .HasDefaultValueSql("'{}'::text[]");

            // Hire car UI caps (new)
            e.Property(p => p.DefaultCarClass).HasMaxLength(64);
            e.Property(p => p.MaxCarClass).HasMaxLength(64);
            e.Property(p => p.MaxCarDailyRate).HasColumnType("numeric(14,2)");

            // Other money caps already annotated; keep numeric(14,2) if you want to enforce here as well:
            // e.Property(p => p.MaxHotelNightlyRate).HasColumnType("numeric(14,2)");
            // e.Property(p => p.MaxTrainPrice).HasColumnType("numeric(14,2)");
            // e.Property(p => p.L1ApprovalAmount).HasColumnType("numeric(14,2)");
            // e.Property(p => p.L2ApprovalAmount).HasColumnType("numeric(14,2)");
            // e.Property(p => p.L3ApprovalAmount).HasColumnType("numeric(14,2)");

            // Policy window (new)
            e.Property(p => p.EffectiveFromUtc).HasColumnType("timestamptz");
            e.Property(p => p.ExpiresOnUtc).HasColumnType("timestamptz");

            // Auditing (new)
            e.Property(p => p.CreatedAtUtc)
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("timezone('utc', now())")
                .ValueGeneratedOnAdd();

            // Prevent updates to CreatedAtUtc after insert
            e.Property(p => p.CreatedAtUtc)
                .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

            e.Property(p => p.LastUpdatedUtc)
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("timezone('utc', now())");
        });

        // ===========================
        // FlightViewOption
        // ===========================
        builder.Entity<FlightViewOption>(e =>
        {
            e.ToTable("flight_view_options", "ava");
            e.HasKey(x => x.Id);

            // columns
            e.Property(x => x.QuoteId)
                .IsRequired()
                .HasColumnName("quote_id");

            // JSONB columns
            e.Property(x => x.Amenities)
                .HasColumnType("jsonb")
                .HasColumnName("amenities");               // List<Amenity>

            e.Property(x => x.Legs)
                .HasColumnType("jsonb")
                .HasColumnName("legs");                    // List<FlightLeg>

            e.Property(x => x.AmadeusFlightOffer)
                .HasColumnType("jsonb")
                .HasColumnName("amadeus_flight_offer");    // FlightOffer?

            // fast lookup by QuoteId
            e.HasIndex(x => x.QuoteId)
                .HasDatabaseName("ix_fvo_quote_id");

            // view-only
            e.Ignore(x => x.DisplayCarriers);
            e.Ignore(x => x.TotalDurationText);
            e.Ignore(x => x.InclusionBadges);
            e.Ignore(x => x.SearchText);
        });

        // ===========================
        // Geography hierarchy
        // ===========================
        builder.Entity<Region>(e =>
        {
            e.ToTable("regions", "ava");
            e.HasKey(x => x.Id);

            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).IsRequired().HasMaxLength(16);
        });
        
        builder.Entity<Continent>(e =>
        {
            e.ToTable("continents", "ava");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired().HasMaxLength(32);
            e.Property(x => x.IsoCode).IsRequired().HasMaxLength(2);

            e.HasIndex(x => x.Name).IsUnique();
            e.HasIndex(x => x.IsoCode).IsUnique();
            e.HasIndex(x => x.RegionId);
        });

        builder.Entity<Country>(e =>
        {
            e.ToTable("countries", "ava");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired().HasMaxLength(128);
            e.Property(x => x.IsoCode).IsRequired().HasMaxLength(3);
            e.Property(x => x.Flag).IsRequired();

            e.HasIndex(x => x.Name).IsUnique();
            e.HasIndex(x => x.IsoCode).IsUnique();
            e.HasIndex(x => x.ContinentId);
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

            e.HasIndex(x => x.Iata);
            e.HasIndex(x => x.Icao);

            e.Property(x => x.Iata).HasMaxLength(2);  // IATA = 2 chars
            e.Property(x => x.Icao).HasMaxLength(3);  // ICAO = 3 chars
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Alias).HasMaxLength(200);
            e.Property(x => x.CallSign).HasMaxLength(200);
            e.Property(x => x.Country).HasMaxLength(100).IsRequired();
            e.Property(x => x.Alliance).HasConversion<int>();
            e.Property(x => x.FoundedYear);
        });
                
        builder.Entity<LoyaltyProgram>(e =>
        {
            e.ToTable("loyalty_programs", "ava");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.AirlineId).IsUnique(); // 1:0..1
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Alliance).HasConversion<int>();

            e.HasOne(x => x.Airline)
             .WithOne(a => a.LoyaltyProgram)
             .HasForeignKey<LoyaltyProgram>(x => x.AirlineId)
             .OnDelete(DeleteBehavior.Cascade);
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

        // ===========================
        // Travel Bookings
        // ===========================
        builder.Entity<TravelQuote>(e =>
        {
            e.ToTable("travel_quotes", "ava");
            e.HasKey(x => x.Id);

            // client org
            e.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // creator
            e.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // assigned TMC
            e.HasOne(x => x.TmcAssigned)
                .WithMany()
                .HasForeignKey(x => x.TmcAssignedId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.CreatedAtUtc)
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(
                    Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

            e.Property(x => x.Alliances)
                .HasColumnType("integer[]")
                .HasConversion(
                    v => v == null ? null : v.Select(a => (int)a).ToArray(),           // List<Alliance> -> int[] or null
                    v => v == null ? null : v.Select(i => (AirlineAlliance)i).ToList()        // int[] -> List<Alliance> or null
                );
        });

        builder.Entity<TravelQuoteUser>(e =>
        {
            e.ToTable("travel_quote_users", "ava");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.TravelQuote)
                .WithMany(q => q.Travellers)
                .HasForeignKey(x => x.TravelQuoteId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
