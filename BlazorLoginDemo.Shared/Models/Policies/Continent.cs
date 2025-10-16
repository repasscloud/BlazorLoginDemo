using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Models.Policies;

[Index(nameof(Name), IsUnique = true)]
[Index(nameof(IsoCode), IsUnique = true)]
public class Continent
{
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string IsoCode { get; set; }

    // Each continent belongs to a region
    public int? RegionId { get; set; }
}
