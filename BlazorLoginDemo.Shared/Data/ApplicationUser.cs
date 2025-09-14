using BlazorLoginDemo.Shared.Models.Kernel.Platform;
using BlazorLoginDemo.Shared.Models.Static.Platform;
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

    // new tenant anchor
    public string? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public UserCategoryType UserCategory { get; set; }

    // Personal data
    [PersonalData, MaxLength(16)]
    public string? PreferredCulture { get; set; } = "en-AU"; // e.g. "en", "en-AU", "es"

    public BlazorLoginDemo.Shared.Models.User.AvaUser? Profile { get; set; }

    // convenience
    public string? TmcId => Organization?.Type switch
    {
        OrganizationType.Tmc => Organization.Id,
        OrganizationType.Client => Organization.ParentOrganizationId,
        _ => null
    };

    public string? ClientId => Organization?.Type == OrganizationType.Client ? Organization.Id : null;
}
