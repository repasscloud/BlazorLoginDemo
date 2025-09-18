using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Models.Policies;

[Index(nameof(Name), IsUnique = true)]
[Index(nameof(IsoCode), IsUnique = true)]
public class Country
{
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string IsoCode { get; set; }

    [Required]
    public required string Flag { get; set; }

    // Each country is part of a continent
    public int? ContinentId { get; set; }

    [JsonIgnore]
    public Continent? Continent { get; set; }
}
