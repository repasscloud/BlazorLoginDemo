using Microsoft.AspNetCore.Identity;

namespace BlazorLoginDemo.Web.Data;


// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastSeenUtc { get; set; }

    // NEW: DB-backed group
    public Guid? GroupId { get; set; }
    public Group? Group { get; set; }
}

