using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Cinturon360.Shared.Helpers;

namespace Cinturon360.Shared.Models.Kernel.SysVar;

public class StorageEntry
{
    [Key]
    [JsonPropertyName("id")]
    [Required]
    [MaxLength(24)]
    public string Id { get; set; } = IDGeneratorHelper.GenerateId(IdGenType.Storage);

    [JsonPropertyName("serializedData")]
    [Required]
    public required string SerializedData { get; set; }

    [JsonPropertyName("expires")]

    public DateTime Expires { get; set; } = DateTime.UtcNow.AddDays(3);
}
