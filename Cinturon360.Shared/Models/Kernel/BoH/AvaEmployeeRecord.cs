using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cinturon360.Shared.Models.Static.Jwt;
using NanoidDotNet;

namespace Cinturon360.Shared.Models.Kernel.BoH;

public class AvaEmployeeRecord
{
    [Key]
    public string Id { get; set; } = Nanoid.Generate();

    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public string PrivateKey { get; set; } = Nanoid.Generate();

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    [Required]
    [Column(TypeName = "varchar(20)")] // Ensures string storage
    [RegularExpression("^(AvaEmployee|AvaAgent|AvaExternal|AvaContractor|System)$", 
        ErrorMessage = "EmployeeType must be AvaEmployee, AvaAgent, AvaExternal, or AvaContractor.")]
    public required string EmployeeType { get; set; }

    public InternalRole Role { get; set; }
    public string? PasswordHash { get; set; }
    public string? VerificationToken { get; set; }
}
