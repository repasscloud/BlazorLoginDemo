using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Cinturon360.Shared.Models.Kernel.Platform;

public sealed class OrganizationDomainUnified
{
    [Key]
    public string Id { get; set; } = NanoidDotNet.Nanoid.Generate();

    [Required, MaxLength(190)]
    public required string Domain { get; set; }

    [Required]
    public string OrganizationUnifiedId { get; set; } = default!;

    [JsonIgnore]
    public OrganizationUnified Organization { get; set; } = default!;
}