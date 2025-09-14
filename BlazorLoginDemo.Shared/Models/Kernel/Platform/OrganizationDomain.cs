using System.ComponentModel.DataAnnotations;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Models.Kernel.Platform;

public sealed class OrganizationDomain
{
    [Key]
    public string Id { get; set; } = Nanoid.Generate();

    [Required]
    [MaxLength(190)]
    public required string Domain { get; set; }
    public string OrganizationId { get; set; } = default!;
    public Organization Organization { get; set; } = default!;
}