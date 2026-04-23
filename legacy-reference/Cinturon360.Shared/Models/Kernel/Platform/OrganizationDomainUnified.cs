using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Cinturon360.Shared.Helpers;

namespace Cinturon360.Shared.Models.Kernel.Platform;

public sealed class OrganizationDomainUnified
{
    [Key]
    [MaxLength(22)]
    public string Id { get; private set; } = IDGeneratorHelper.GenerateId(IdGenType.Domain);

    [Required, MaxLength(190)]
    public required string Domain { get; set; }

    [Required]
    public string OrganizationUnifiedId { get; set; } = default!;

    [JsonIgnore]
    public OrganizationUnified Organization { get; set; } = default!;
}