using Microsoft.AspNetCore.Mvc;
using BlazorLoginDemo.Shared.Services.Interfaces.Platform;
using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Security;
using BlazorLoginDemo.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BlazorLoginDemo.Shared.Models.Kernel.Billing;
using BlazorLoginDemo.Shared.Models.Static.Billing;
using static BlazorLoginDemo.Shared.Models.Kernel.Billing.LicenseAgreementUnified;
//using BlazorLoginDemo.Shared.Services.Interfaces.Platform;

namespace BlazorLoginDemo.Api.Controllers.Test;

[Route("api/v1/test")]
// [ServiceFilter(typeof(RequireApiKeyFilter))]
[ApiController]
public class TestDataController : ControllerBase
{
    private readonly IAdminOrgServiceUnified _adminOrgServiceUnified;
    private readonly IAdminUserServiceUnified _adminUserServiceUnified;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdminLicenseAgreementServiceUnified _licenseSvc;

    public TestDataController(
        IAdminOrgServiceUnified adminOrgServiceUnified,
        IAdminUserServiceUnified adminUserServiceUnified,
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IAdminLicenseAgreementServiceUnified licenseSvc)
    {
        _adminOrgServiceUnified = adminOrgServiceUnified;
        _adminUserServiceUnified = adminUserServiceUnified;
        _db = db;
        _userManager = userManager;
        _licenseSvc = licenseSvc;
    }

    private sealed class CreateOrganizationVm
    {
        [Required, StringLength(128)] public string Name { get; set; } = string.Empty;
        [Required] public Shared.Models.Static.Platform.OrganizationType Type { get; set; } = Shared.Models.Static.Platform.OrganizationType.Client;
        public string? ParentOrganizationId { get; set; }
        public bool IsActive { get; set; } = true;
        [MinLength(0)] public List<string> Domains { get; set; } = new();
        [RegularExpression(@"^[A-Za-z0-9\-]+(\.[A-Za-z0-9\-]+)+$", ErrorMessage = "Enter a valid domain like example.com")]
        public string? DomainPattern => null; // not used; pattern is referenced via ValidationMessage for each Domains[i]
    }

    [HttpGet("create-org-data")]
    public async Task<IActionResult> SetupTestOrgData()
    {
        var _vm = new CreateOrganizationVm();

        _vm.Name = "RePass Cloud Pty Ltd";
        _vm.Type = Shared.Models.Static.Platform.OrganizationType.Vendor;
        _vm.ParentOrganizationId = null;
        _vm.IsActive = true;
        _vm.Domains = new List<string> { "repasscloud.com" };

        var req2 = new IAdminOrgServiceUnified.CreateOrgRequest(
            Name: _vm.Name.Trim(),
            Type: _vm.Type, // enum value
            ParentOrganizationId: string.IsNullOrWhiteSpace(_vm.ParentOrganizationId) ? null : _vm.ParentOrganizationId.Trim(),
            IsActive: _vm.IsActive,
            Domains: _vm.Domains.Select(d => d.Trim().ToLowerInvariant()).Where(d => !string.IsNullOrWhiteSpace(d)).Distinct().ToList()
        );

        IAdminOrgServiceUnified.OrgAggregate? aggregate2 = await _adminOrgServiceUnified.CreateAsync(req2);


        _vm.Name = "New World Travel Management";
        _vm.Type = Shared.Models.Static.Platform.OrganizationType.Tmc;
        _vm.ParentOrganizationId = null;
        _vm.IsActive = true;
        _vm.Domains = new List<string> { "nwtravelmgmt.com.au" };

        var req3 = new IAdminOrgServiceUnified.CreateOrgRequest(
            Name: _vm.Name.Trim(),
            Type: _vm.Type, // enum value
            ParentOrganizationId: string.IsNullOrWhiteSpace(_vm.ParentOrganizationId) ? null : _vm.ParentOrganizationId.Trim(),
            IsActive: _vm.IsActive,
            Domains: _vm.Domains.Select(d => d.Trim().ToLowerInvariant()).Where(d => !string.IsNullOrWhiteSpace(d)).Distinct().ToList()
        );

        IAdminOrgServiceUnified.OrgAggregate? aggregate3 = await _adminOrgServiceUnified.CreateAsync(req3);


        _vm.Name = "MCDONALD'S AUSTRALIA LIMITED";
        _vm.Type = Shared.Models.Static.Platform.OrganizationType.Client;
        _vm.ParentOrganizationId = null;
        _vm.IsActive = true;
        _vm.Domains = new List<string> { "mcdonalds.com.au", "maccas.com" };

        var req4 = new IAdminOrgServiceUnified.CreateOrgRequest(
            Name: _vm.Name.Trim(),
            Type: _vm.Type, // enum value
            ParentOrganizationId: string.IsNullOrWhiteSpace(_vm.ParentOrganizationId) ? null : _vm.ParentOrganizationId.Trim(),
            IsActive: _vm.IsActive,
            Domains: _vm.Domains.Select(d => d.Trim().ToLowerInvariant()).Where(d => !string.IsNullOrWhiteSpace(d)).Distinct().ToList()
        );

        IAdminOrgServiceUnified.OrgAggregate? aggregate4 = await _adminOrgServiceUnified.CreateAsync(req4);

        return Ok(new { ok = true, now = DateTime.UtcNow });
    }

    private sealed class CreateUserVm
    {
        [Required, EmailAddress] public string? Email { get; set; }

        [Required, MinLength(8)] public string? Password { get; set; }

        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; }

        public string? OrganizationId { get; set; }
        public string? RoleName { get; set; } = "Client.Requestor";
        public string? ManagerUserId { get; set; }
    }

    [HttpGet("create-user-data")]
    public async Task<IActionResult> SetupTestUserData()
    {
        // repass cloud is not needed, it's created as part of initial seeding

        // nwtravelmgmt
        var vm1 = new CreateUserVm
        {
            Email = "testuser@nwtravelmgmt.com.au",
            Password = "P@ssword123",
            FirstName = "testuser",
            MiddleName = null,
            LastName = "nwtravelmgmt",
            DisplayName = "nwtravelmgmt Test User",
            OrganizationId = await _db.Organizations.Where(o => o.Name == "New World Travel Management").Select(o => o.Id).AsNoTracking().FirstOrDefaultAsync(),
            RoleName = "Client.Requestor",
            ManagerUserId = null
        };

        var req1 = new IAdminUserServiceUnified.CreateUserRequest(
            Email: vm1.Email!,
            Password: vm1.Password!,
            FirstName: vm1.FirstName,
            MiddleName: vm1.MiddleName,
            LastName: vm1.LastName,
            DisplayName: vm1.DisplayName,
            OrganizationId: string.IsNullOrWhiteSpace(vm1.OrganizationId) ? null : vm1.OrganizationId,
            RoleName: string.IsNullOrWhiteSpace(vm1.RoleName) ? null : vm1.RoleName,
            ManagerUserId: string.IsNullOrWhiteSpace(vm1.ManagerUserId) ? null : vm1.ManagerUserId
        );

        var result1 = await _adminUserServiceUnified.CreateUserAsync(req1);

        // maccas
        var vm2 = new CreateUserVm
        {
            Email = "testuser@maccas.com",
            Password = "P@ssword123",
            FirstName = "testuser",
            MiddleName = null,
            LastName = "maccas",
            DisplayName = "maccas Test User",
            OrganizationId = await _db.Organizations.Where(o => o.Name == "MCDONALD'S AUSTRALIA LIMITED").Select(o => o.Id).AsNoTracking().FirstOrDefaultAsync(),
            RoleName = "Client.Requestor",
            ManagerUserId = null
        };

        var req2 = new IAdminUserServiceUnified.CreateUserRequest(
            Email: vm2.Email!,
            Password: vm2.Password!,
            FirstName: vm2.FirstName,
            MiddleName: vm2.MiddleName,
            LastName: vm2.LastName,
            DisplayName: vm2.DisplayName,
            OrganizationId: string.IsNullOrWhiteSpace(vm2.OrganizationId) ? null : vm2.OrganizationId,
            RoleName: string.IsNullOrWhiteSpace(vm2.RoleName) ? null : vm2.RoleName,
            ManagerUserId: string.IsNullOrWhiteSpace(vm2.ManagerUserId) ? null : vm2.ManagerUserId
        );

        var result2 = await _adminUserServiceUnified.CreateUserAsync(req2);

        var usrId1 = _userManager.FindByEmailAsync(vm1.Email!).Result?.Id;
        var usrId2 = _userManager.FindByEmailAsync(vm2.Email!).Result?.Id;

        return Ok(new
        {
            ok = "OK",
            userId1 = usrId1,
            userId2 = usrId2,
            now = DateTime.UtcNow
        });
    }
    
    [HttpGet("create-org-license")]
    public async Task<IActionResult> CreateOrgLicenses(CancellationToken ct)
    {
        // Look up org IDs by name as created in your other test routes
        var platformOrgId = await _db.Organizations
            .Where(o => o.Name == "RePass Cloud Pty Ltd")
            .Select(o => o.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        var tmcOrgId = await _db.Organizations
            .Where(o => o.Name == "New World Travel Management")
            .Select(o => o.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        var clientOrgId = await _db.Organizations
            .Where(o => o.Name == "MCDONALD'S AUSTRALIA LIMITED")
            .Select(o => o.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrWhiteSpace(platformOrgId) ||
            string.IsNullOrWhiteSpace(tmcOrgId) ||
            string.IsNullOrWhiteSpace(clientOrgId))
        {
            return BadRequest(new { ok = false, error = "Required organizations not found" });
        }

        // Common dates
        var start = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var expiry = start.AddYears(1);

        // 1) License for Platform itself (issuer = Platform)
        var modelPlatform = new LicenseAgreementUnified
        {
            OrganizationUnifiedId = platformOrgId,
            CreatedByOrganizationUnifiedId = platformOrgId,
            StartDate = start,
            ExpiryDate = expiry,
            AutoRenew = true,
            BillingType = BillingType.Prepaid,
            BillingFrequency = BillingFrequency.Monthly,
            AccessFee = 0m,
            AccessFeeScope = BillingPeriodScope.Monthly,
            PaymentStatus = PaymentStatus.Pending,
            PrepaidBalance = 0m,
            GracePeriodDays = 0
        };

        // 2) License for TMC (issuer = Platform)
        var modelTmc = new LicenseAgreementUnified
        {
            OrganizationUnifiedId = tmcOrgId,
            CreatedByOrganizationUnifiedId = platformOrgId,
            StartDate = start,
            ExpiryDate = expiry,
            AutoRenew = true,
            BillingType = BillingType.Prepaid,
            BillingFrequency = BillingFrequency.Monthly,
            AccessFee = 199m,
            AccessFeeScope = BillingPeriodScope.Monthly,
            PaymentStatus = PaymentStatus.Pending,
            PrepaidBalance = 1000m,
            GracePeriodDays = 7
        };

        // 3) License for Client (issuer = TMC)
        var modelClient = new LicenseAgreementUnified
        {
            OrganizationUnifiedId = clientOrgId,
            CreatedByOrganizationUnifiedId = tmcOrgId,
            StartDate = start,
            ExpiryDate = expiry,
            AutoRenew = true,
            BillingType = BillingType.Postpaid,
            BillingFrequency = BillingFrequency.Monthly,
            AccessFee = 49m,
            AccessFeeScope = BillingPeriodScope.Monthly,
            PaymentStatus = PaymentStatus.Pending,
            MinimumMonthlySpend = 500m,
            GracePeriodDays = 5
        };

        // Use Upsert for idempotent test seeding
        var agg1 = await _licenseSvc.UpsertForOrganizationAsync(
            modelPlatform.OrganizationUnifiedId,
            modelPlatform.CreatedByOrganizationUnifiedId,
            modelPlatform, ct);

        var agg2 = await _licenseSvc.UpsertForOrganizationAsync(
            modelTmc.OrganizationUnifiedId,
            modelTmc.CreatedByOrganizationUnifiedId,
            modelTmc, ct);

        var agg3 = await _licenseSvc.UpsertForOrganizationAsync(
            modelClient.OrganizationUnifiedId,
            modelClient.CreatedByOrganizationUnifiedId,
            modelClient, ct);

        return Ok(new
        {
            ok = true,
            platformLicenseId = agg1.License.Id,
            tmcLicenseId = agg2.License.Id,
            clientLicenseId = agg3.License.Id,
            now = DateTime.UtcNow
        });
    }
}