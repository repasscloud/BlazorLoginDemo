using BlazorLoginDemo.Shared.Models.Kernel.User;
using BlazorLoginDemo.Shared.Models.User;
using BlazorLoginDemo.Shared.Services.Interfaces.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorLoginDemo.Shared.Services.User;

internal sealed class AdminUserService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AdminUserService> _log;

    public AdminUserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db,
        ILogger<AdminUserService> log)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _log = log;
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

    public async Task<IAdminUserService.DeleteUserResult> DeleteByAspNetUserIdAsync(string aspNetUserId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(aspNetUserId))
                return new(false, "User Id is required.");

            // 1) Load Identity user + child rows (typical Identity tables)
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == aspNetUserId, ct);

            if (user is null)
                return new(true, null); // already gone

            var claims  = await _db.UserClaims.Where(x => x.UserId == aspNetUserId).ToListAsync(ct);
            var roles   = await _db.UserRoles.Where(x => x.UserId == aspNetUserId).ToListAsync(ct);
            var logins  = await _db.UserLogins.Where(x => x.UserId == aspNetUserId).ToListAsync(ct);
            var tokens  = await _db.UserTokens.Where(x => x.UserId == aspNetUserId).ToListAsync(ct);

            // 2) Load your domain entities tied to this user
            //    Assumes AvaUser has a FK like IdentityUserId (adjust if named differently).
            var avaUser = await _db.Set<AvaUser>()
                .FirstOrDefaultAsync(x => x.AspNetUsersId == aspNetUserId, ct);

            if (avaUser is not null)
            {
                var prefs = await _db.Set<AvaUserSysPreference>()
                    .Where(p => p.AvaUserId == avaUser.Id)
                    .ToListAsync(ct);

                _db.Set<AvaUserSysPreference>().RemoveRange(prefs);
                _db.Set<AvaUser>().Remove(avaUser);
            }

            // 3) Remove Identity child rows then the user
            _db.UserClaims.RemoveRange(claims);
            _db.UserRoles.RemoveRange(roles);
            _db.UserLogins.RemoveRange(logins);
            _db.UserTokens.RemoveRange(tokens);
            _db.Users.Remove(user);

            await _db.SaveChangesAsync(ct);
            return new(true, null);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "DeleteByAspNetUserIdAsync failed for {UserId}", aspNetUserId);
            return new(false, ex.GetBaseException().Message);
        }
    }

    public async Task<IAdminUserService.DeleteUserResult> DeleteByAvaUserIdAsync(string avaUserId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(avaUserId))
                return new(false, "AvaUser Id is required.");

            // 1) Load AvaUser + prefs
            var avaUser = await _db.Set<AvaUser>()
                .FirstOrDefaultAsync(x => x.Id == avaUserId, ct);

            if (avaUser is null)
                return new(true, null); // already gone

            var prefs = await _db.Set<AvaUserSysPreference>()
                .Where(p => p.AvaUserId == avaUser.Id)
                .ToListAsync(ct);

            // 2) Optionally load Identity user via FK (adjust property name if needed)
            IdentityUser? user = null;
            if (!string.IsNullOrWhiteSpace(avaUser.AspNetUsersId))
            {
                var uid = avaUser.AspNetUsersId!;
                user = await _db.Users.FirstOrDefaultAsync(u => u.Id == uid, ct);

                var claims  = await _db.UserClaims.Where(x => x.UserId == uid).ToListAsync(ct);
                var roles   = await _db.UserRoles.Where(x => x.UserId == uid).ToListAsync(ct);
                var logins  = await _db.UserLogins.Where(x => x.UserId == uid).ToListAsync(ct);
                var tokens  = await _db.UserTokens.Where(x => x.UserId == uid).ToListAsync(ct);

                _db.UserClaims.RemoveRange(claims);
                _db.UserRoles.RemoveRange(roles);
                _db.UserLogins.RemoveRange(logins);
                _db.UserTokens.RemoveRange(tokens);

                if (user is not null)
                    _db.Users.Remove((ApplicationUser)user);
            }

            // 3) Remove domain rows
            _db.Set<AvaUserSysPreference>().RemoveRange(prefs);
            _db.Set<AvaUser>().Remove(avaUser);

            await _db.SaveChangesAsync(ct);
            return new(true, null);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "DeleteByAvaUserIdAsync failed for {AvaUserId}", avaUserId);
            return new(false, ex.GetBaseException().Message);
        }
    }

    // --- UTIL ---
    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
    {
        return await _db.Set<ApplicationUser>().AnyAsync(u => u.Id == id, ct);
    }
}
