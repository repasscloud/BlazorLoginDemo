using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using Cinturon360.Domain.Entities.Identity;
using Cinturon360.Domain.Entities.Organization;
using Cinturon360.Domain.Entities.Travel;
using Cinturon360.Domain.Entities.Booking;
using Cinturon360.Domain.Entities.Policy;
using Cinturon360.Domain.Entities.Approval;
using Cinturon360.Domain.Entities.Billing;
using Cinturon360.Domain.Entities.Pricing;
using Cinturon360.Domain.Entities.System;
using Cinturon360.Domain.Entities.Ticketing;

namespace Cinturon360.Data.Context;

/// <summary>
/// Single EF Core DbContext for Cinturon360.
/// All entity configurations are loaded from the Configurations/ folder.
/// OpenIddict tables are registered via UseOpenIddict().
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Identity ─────────────────────────────────────────────────────────
    public DbSet<User> Users => Set<User>();
    public DbSet<UserAuthMethod> UserAuthMethods => Set<UserAuthMethod>();
    public DbSet<UserSecurity> UserSecurities => Set<UserSecurity>();
    public DbSet<UserMfaMethod> UserMfaMethods => Set<UserMfaMethod>();
    public DbSet<UserRecoveryCode> UserRecoveryCodes => Set<UserRecoveryCode>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserApiToken> UserApiTokens => Set<UserApiToken>();
    public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();
    public DbSet<UserAuditEvent> UserAuditEvents => Set<UserAuditEvent>();
    public DbSet<UserProvisioningSource> UserProvisioningSources => Set<UserProvisioningSource>();
    public DbSet<UserAccessOverride> UserAccessOverrides => Set<UserAccessOverride>();

    // ── Traveller profile ─────────────────────────────────────────────────
    public DbSet<TravellerProfile> TravellerProfiles => Set<TravellerProfile>();
    public DbSet<TravellerLoyaltyProgram> TravellerLoyaltyPrograms => Set<TravellerLoyaltyProgram>();
    public DbSet<UserEmergencyContact> UserEmergencyContacts => Set<UserEmergencyContact>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();

    // ── Organization ─────────────────────────────────────────────────────
    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // ── Geography ─────────────────────────────────────────────────────────
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Airport> Airports => Set<Airport>();

    // ── Bookings ──────────────────────────────────────────────────────────
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<Quote> Quotes => Set<Quote>();

    // ── Policy ────────────────────────────────────────────────────────────
    public DbSet<TravelPolicy> TravelPolicies => Set<TravelPolicy>();
    public DbSet<PolicyRule> PolicyRules => Set<PolicyRule>();
    public DbSet<PolicyAssignment> PolicyAssignments => Set<PolicyAssignment>();

    // ── Approvals ─────────────────────────────────────────────────────────
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<ApprovalDecision> ApprovalDecisions => Set<ApprovalDecision>();

    // ── Billing ───────────────────────────────────────────────────────────
    public DbSet<OrgLicense> OrgLicenses => Set<OrgLicense>();
    public DbSet<OrgBillingConfig> OrgBillingConfigs => Set<OrgBillingConfig>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PrepaidBalance> PrepaidBalances => Set<PrepaidBalance>();

    // ── System ────────────────────────────────────────────────────────────
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<StoredDocument> StoredDocuments => Set<StoredDocument>();
    // ── Exchange Rates ────────────────────────────────────────────────────
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    // ── Support Ticketing ─────────────────────────────────────────────────
    public DbSet<SupportTicket>    SupportTickets    => Set<SupportTicket>();
    public DbSet<TicketComment>    TicketComments    => Set<TicketComment>();
    public DbSet<TicketEscalation> TicketEscalations => Set<TicketEscalation>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<TicketEmailTemplate> TicketEmailTemplates => Set<TicketEmailTemplate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Register all entity type configurations from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // OpenIddict tables
        builder.UseOpenIddict();
    }
}
