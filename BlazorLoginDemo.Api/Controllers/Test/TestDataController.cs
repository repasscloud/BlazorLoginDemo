using Microsoft.AspNetCore.Mvc;
using BlazorLoginDemo.Shared.Services.Interfaces.Platform;
using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Security;
using BlazorLoginDemo.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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

    public TestDataController(
        IAdminOrgServiceUnified adminOrgServiceUnified,
        IAdminUserServiceUnified adminUserServiceUnified,
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _adminOrgServiceUnified = adminOrgServiceUnified;
        _adminUserServiceUnified = adminUserServiceUnified;
        _db = db;
        _userManager = userManager;
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
}