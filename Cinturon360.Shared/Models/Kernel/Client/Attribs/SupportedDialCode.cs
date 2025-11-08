using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Models.Kernel.Client.Attribs;

public class SupportedDialCode
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string CountryCode { get; set; }

    [Required]
    public required string CountryName { get; set; }
}
