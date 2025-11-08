using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Models.Kernel.Client.Attribs;

public class SupportedTaxId
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string TaxIdType { get; set; }
}
