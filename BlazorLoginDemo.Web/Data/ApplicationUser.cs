using Microsoft.AspNetCore.Identity;

namespace BlazorLoginDemo.Web.Data;

public enum UserGroup
{
    Guest = 0,
    Member = 1,
    Manager = 2,
    Admin = 3
}

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    // Store as string in DB via EF conversion (see DbContext config below)
    public UserGroup Group { get; set; } = UserGroup.Guest;

    public string? Department { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastSeenUtc { get; set; }
}

