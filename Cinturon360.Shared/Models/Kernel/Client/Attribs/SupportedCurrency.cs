using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Models.Kernel.Client.Attribs;

public class SupportedCurrency
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(3)]
    public required string Iso4217 { get; set; }

    [Required]
    public required string Symbol { get; set; }

    [Required]
    public required string Name { get; set; }
}
