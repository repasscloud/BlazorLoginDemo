using BlazorLoginDemo.Shared.Models.User;
using BlazorLoginDemo.Shared.Services.Interfaces.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.User;

internal sealed class AdminUserService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public AdminUserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    // --- CREATE (rich/typed) ---
    public async Task<IAdminUserService.ProvisionedUser> CreateAsync(
        IAdminUserService.CreateUserRequest req,
        CancellationToken ct = default)
    {
        // Ensure role exists (bootstrap-friendly)
        if (!await _roleManager.RoleExistsAsync(req.RoleName))
        {
            var rc = await _roleManager.CreateAsync(new IdentityRole(req.RoleName));
            if (!rc.Succeeded)
                throw new InvalidOperationException(string.Join("; ", rc.Errors.Select(e => $"{e.Code}:{e.Description}")));
        }

        // Guard: email
        var email = req.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(req.Email));

        // Guard: duplicate email
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
            throw new InvalidOperationException($"An account with {email} already exists.");

        // 1) Create Identity user
        var appUser = new ApplicationUser
        {
            UserName       = email,
            Email          = email,
            EmailConfirmed = true, // for first-time bootstrap; flip later if you want confirmations
            IsActive       = true,
            FirstName      = req.FirstName,
            MiddleName     = req.MiddleName,
            LastName       = req.LastName,
            DisplayName    = string.IsNullOrWhiteSpace(req.DisplayName)
                                ? $"{req.FirstName} {req.LastName}".Trim()
                                : req.DisplayName,
            OrganizationId = req.OrganizationId,
            LastSeenUtc    = DateTimeOffset.UtcNow
        };

        var createResult = await _userManager.CreateAsync(appUser, req.Password);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(e => $"{e.Code}:{e.Description}")));

        // Compensating action helper
        async Task CleanupIdentityAsync()
        {
            try { await _userManager.RemoveFromRoleAsync(appUser, req.RoleName); } catch { /* ignore */ }
            try { await _userManager.DeleteAsync(appUser); } catch { /* ignore */ }
        }

        // 2) Assign role
        var roleResult = await _userManager.AddToRoleAsync(appUser, req.RoleName);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(appUser);
            throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(e => $"{e.Code}:{e.Description}")));
        }

        // 3) Create AvaUser (domain profile) linked by AspNetUsersId (1:1)
        var profile = new AvaUser
        {
            AspNetUsersId        = appUser.Id, // strict 1:1 link to Identity
            FirstName            = req.FirstName ?? string.Empty,
            MiddleName           = req.MiddleName,
            LastName             = req.LastName ?? string.Empty,
            Email                = email,

            // sensible defaults aligned with your model
            DefaultCurrencyCode  = "AUD",
            DefaultFlightSeating = "ECONOMY",
            MaxFlightSeating     = "ECONOMY",
            NonStopFlight        = false,
            MaxFlightPrice       = 0,

            // optional line-approval manager (self-FK on AvaUser)
            ManagerAvaUserId     = req.ManagerAvaUserId
        };

        try
        {
            _db.Set<AvaUser>().Add(profile);
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            await CleanupIdentityAsync();
            throw;
        }

        // Load composite for return
        var identity = await _db.Set<ApplicationUser>()
            .Include(u => u.Profile)
            .FirstAsync(u => u.Id == appUser.Id, ct);

        if (identity.Profile is null)
        {
            // Extremely unlikely given we just created it; defensive
            await CleanupIdentityAsync();
            throw new InvalidOperationException("Profile creation failed.");
        }

        return new IAdminUserService.ProvisionedUser(identity.Id, identity, identity.Profile);
    }

    // --- CREATE (UI-friendly result) ---
    public async Task<IAdminUserService.CreateUserResult> CreateUserAsync(
        IAdminUserService.CreateUserRequest req,
        CancellationToken ct = default)
    {
        try
        {
            var provisioned = await CreateAsync(req, ct);
            return new(true, null, provisioned.UserId);
        }
        catch (Exception ex)
        {
            // Roll up the error for UI surfaces
            return new(false, ex.GetBaseException().Message, null);
        }
    }

    // --- READ ---
    public async Task<IAdminUserService.ProvisionedUser?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _db.Set<ApplicationUser>()
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        return user?.Profile is null
            ? null
            : new IAdminUserService.ProvisionedUser(user.Id, user, user.Profile);
    }

    public async Task<IReadOnlyList<IAdminUserService.ProvisionedUser>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _db.Set<ApplicationUser>()
            .Include(u => u.Profile)
            .OrderBy(u => u.Email)
            .ToListAsync(ct);

        return users
            .Where(u => u.Profile is not null)
            .Select(u => new IAdminUserService.ProvisionedUser(u.Id, u, u.Profile!))
            .ToList();
    }

    // --- UPDATE ---
    public async Task<IAdminUserService.ProvisionedUser> UpdateAsync(
        IAdminUserService.UpdateUserRequest req,
        CancellationToken ct = default)
    {
        var user = await _db.Set<ApplicationUser>()
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == req.UserId, ct);

        if (user is null || user.Profile is null)
            throw new InvalidOperationException($"User '{req.UserId}' not found.");

        // Identity side
        if (req.DisplayName is not null)   user.DisplayName = req.DisplayName;
        if (req.OrganizationId is not null) user.OrganizationId = req.OrganizationId;
        if (req.IsActive.HasValue)         user.IsActive = req.IsActive.Value;

        // Domain profile side
        if (req.FirstName is not null)     user.Profile.FirstName = req.FirstName;
        if (req.MiddleName is not null)    user.Profile.MiddleName = req.MiddleName;
        if (req.LastName is not null)      user.Profile.LastName = req.LastName;
        if (req.ManagerAvaUserId is not null) user.Profile.ManagerAvaUserId = req.ManagerAvaUserId;

        await _db.SaveChangesAsync(ct);

        return new IAdminUserService.ProvisionedUser(user.Id, user, user.Profile);
    }

    // --- DELETE ---
    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    // --- UTIL ---
    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
    {
        return await _db.Set<ApplicationUser>().AnyAsync(u => u.Id == id, ct);
    }
}
