using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Models.Policies;

[Index(nameof(Name), IsUnique = true)]
public class Region
{
    public int Id { get; set; }
    
    [Required]
    public required string Name { get; set; }
}
