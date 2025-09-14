using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Models.Static.Platform;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Models.Kernel.Platform;

public sealed class Organization
{
    [Key]
    public string Id { get; set; } = Nanoid.Generate();
    [Required] public required string Name { get; set; }
    public OrganizationType Type { get; set; }
    public string? ParentOrganizationId { get; set; }
    public ICollection<Organization> Children { get; set; } = new List<Organization>();
    public bool IsActive { get; set; } = true;
    public ICollection<OrganizationDomain> Domains { get; set; } = new List<OrganizationDomain>();

}