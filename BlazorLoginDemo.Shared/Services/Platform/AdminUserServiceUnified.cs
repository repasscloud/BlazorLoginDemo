using BlazorLoginDemo.Shared.Models.Kernel.Platform;
using BlazorLoginDemo.Shared.Services.Interfaces.Platform;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorLoginDemo.Shared.Services.Platform;

internal sealed class AdminUserServiceUnified : IAdminUserServiceUnified
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AdminUserServiceUnified> _log;

    public AdminUserServiceUnified(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AdminUserServiceUnified> log)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _log = log;
    }

    // -------------- CREATE --------------
    public async Task<IAdminUserServiceUnified.UserAggregate> CreateAsync(IAdminUserServiceUnified.CreateUserRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Email)) throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(req.Password)) throw new ArgumentException("Password is required.");

        OrganizationUnified? org = null;
        if (!string.IsNullOrWhiteSpace(req.OrganizationId))
        {
            org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == req.OrganizationId, ct)
                ?? throw new InvalidOperationException($"Organization '{req.OrganizationId}' not found.");
        }

        var user = new ApplicationUser
        {
            UserName = req.Email.Trim().ToLowerInvariant(),
            Email = req.Email.Trim().ToLowerInvariant(),
            FirstName = req.FirstName,
            MiddleName = req.MiddleName,
            LastName = req.LastName,
            DisplayName = req.DisplayName,
            OrganizationId = org?.Id,
            EmailConfirmed = false,
            IsActive = true
        };

        var identityResult = await _userManager.CreateAsync(user, req.Password);
        if (!identityResult.Succeeded)
            throw new InvalidOperationException(string.Join("; ", identityResult.Errors.Select(e => $"{e.Code}:{e.Description}")));

        // Assign role if provided
        if (!string.IsNullOrWhiteSpace(req.RoleName))
        {
            if (!await _roleManager.RoleExistsAsync(req.RoleName))
                await _roleManager.CreateAsync(new IdentityRole(req.RoleName));
            await _userManager.AddToRoleAsync(user, req.RoleName);
        }

        // Manager (self-ref)
        if (!string.IsNullOrWhiteSpace(req.ManagerUserId))
        {
            var manager = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.ManagerUserId, ct)
                ?? throw new InvalidOperationException($"Manager user '{req.ManagerUserId}' not found.");
            user.ManagerId = manager.Id;
            await _db.SaveChangesAsync(ct);
        }

        var loaded = await _db.Users.Include(u => u.Organization).FirstAsync(u => u.Id == user.Id, ct);
        return new IAdminUserServiceUnified.UserAggregate(loaded.Id, loaded, loaded.Organization);
    }

    public async Task<IAdminUserServiceUnified.CreateUserResult> CreateUserAsync(IAdminUserServiceUnified.CreateUserRequest req, CancellationToken ct = default)
    {
        try
        {
            var agg = await CreateAsync(req, ct);
            return new(true, null, agg.UserId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "CreateUserAsync failed");
            return new(false, ex.GetBaseException().Message, null);
        }
    }

    // -------------- READ --------------
    public async Task<IAdminUserServiceUnified.UserAggregate?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _db.Users.Include(u => u.Organization).FirstOrDefaultAsync(u => u.Id == id, ct);
        return user is null ? null : new IAdminUserServiceUnified.UserAggregate(user.Id, user, user.Organization);
    }

    public async Task<IReadOnlyList<IAdminUserServiceUnified.UserAggregate>> SearchAsync(
        string? emailContains,
        string? nameContains,
        string? organizationId,
        bool? isActive,
        CancellationToken ct = default)
    {
        IQueryable<ApplicationUser> q = _db.Users.Include(u => u.Organization).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(emailContains))
            q = q.Where(u => EF.Functions.ILike(u.Email!, $"%{emailContains.Trim()}%"));
        if (!string.IsNullOrWhiteSpace(nameContains))
            q = q.Where(u => EF.Functions.ILike(u.DisplayName!, $"%{nameContains.Trim()}%") ||
                             EF.Functions.ILike(u.FirstName!, $"%{nameContains.Trim()}%") ||
                             EF.Functions.ILike(u.LastName!, $"%{nameContains.Trim()}%"));
        if (!string.IsNullOrWhiteSpace(organizationId))
            q = q.Where(u => u.OrganizationId == organizationId);
        if (isActive.HasValue)
            q = q.Where(u => u.IsActive == isActive.Value);

        var list = await q.OrderBy(u => u.Email).ThenBy(u => u.Id).ToListAsync(ct);
        return list.Select(u => new IAdminUserServiceUnified.UserAggregate(u.Id, u, u.Organization)).ToList();
    }

    // -------------- UPDATE --------------
    public async Task<IAdminUserServiceUnified.UserAggregate> UpdateAsync(IAdminUserServiceUnified.UpdateUserRequest req, CancellationToken ct = default)
    {
        var user = await _db.Users.Include(u => u.Organization).FirstOrDefaultAsync(u => u.Id == req.UserId, ct)
            ?? throw new InvalidOperationException($"User '{req.UserId}' not found.");

        if (req.FirstName is not null) user.FirstName = req.FirstName;
        if (req.MiddleName is not null) user.MiddleName = req.MiddleName;
        if (req.LastName is not null) user.LastName = req.LastName;
        if (req.DisplayName is not null) user.DisplayName = req.DisplayName;
        if (req.IsActive.HasValue) user.IsActive = req.IsActive.Value;

        if (req.OrganizationId is not null)
        {
            if (req.OrganizationId.Length == 0) user.OrganizationId = null;
            else
            {
                var exists = await _db.Organizations.AnyAsync(o => o.Id == req.OrganizationId, ct);
                if (!exists) throw new InvalidOperationException($"Organization '{req.OrganizationId}' not found.");
                user.OrganizationId = req.OrganizationId;
            }
        }

        if (req.ManagerUserId is not null)
        {
            if (req.ManagerUserId.Length == 0) user.ManagerId = null;
            else
            {
                var manager = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.ManagerUserId, ct)
                    ?? throw new InvalidOperationException($"Manager user '{req.ManagerUserId}' not found.");
                if (manager.Id == user.Id) throw new InvalidOperationException("A user cannot be their own manager.");
                user.ManagerId = manager.Id;
            }
        }

        await _db.SaveChangesAsync(ct);
        var loaded = await _db.Users.Include(u => u.Organization).FirstAsync(u => u.Id == user.Id, ct);
        return new IAdminUserServiceUnified.UserAggregate(loaded.Id, loaded, loaded.Organization);
    }

    // -------------- DELETE --------------
    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return false;

        // Reassign direct reports (null-out manager) to avoid delete restriction
        var reports = await _db.Users.Where(u => u.ManagerId == id).ToListAsync(ct);
        foreach (var r in reports) r.ManagerId = null;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
        => await _db.Users.AnyAsync(u => u.Id == id, ct);
}