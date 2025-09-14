using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlazorLoginDemo.Shared.Data;


// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastSeenUtc { get; set; }

    // removed for issue 15
    // // NEW: DB-backed group
    // public Guid? GroupId { get; set; }
    // public Group? Group { get; set; }

    // Personal data
    [PersonalData, MaxLength(16)]
    public string? PreferredCulture { get; set; } = "en-AU"; // e.g. "en", "en-AU", "es"

    public BlazorLoginDemo.Shared.Models.User.AvaUser? Profile { get; set; }
}

