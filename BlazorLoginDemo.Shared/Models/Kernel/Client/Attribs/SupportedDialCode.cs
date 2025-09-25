using System.ComponentModel.DataAnnotations;

namespace BlazorLoginDemo.Shared.Models.Kernel.Client.Attribs;

public class SupportedDialCode
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string CountryCode { get; set; }

    [Required]
    public required string CountryName { get; set; }
}
