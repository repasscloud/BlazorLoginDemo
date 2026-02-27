using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cinturon360.Shared.Helpers;

namespace Cinturon360.Shared.Models.Kernel.BoH;

public class C360EmployeeRecord
{
    [Key]
    [MaxLength(16)]
    public string Id { get; set; } =
        IDGeneratorHelper.GenerateId(IdGenType.Employee);

    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MaxLength(23)]
    public string PrivateKey { get; private set; } =
        IDGeneratorHelper.GenerateId(IdGenType.EmployeePrivateKey);

    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    [Required]
    [Column(TypeName = "varchar(20)")]
    [RegularExpression(
        "^(AvaEmployee|AvaAgent|AvaExternal|AvaContractor|System)$",
        ErrorMessage = "EmployeeType must be AvaEmployee, AvaAgent, AvaExternal, or AvaContractor."
    )]
    public required string EmployeeType { get; set; }

    public string? PasswordHash { get; set; }
    public string? VerificationToken { get; set; }

    // ----------------------------
    // Private key rotation
    // ----------------------------
    private void RotatePrivateKey()
    {
        PrivateKey = IDGeneratorHelper.GenerateId(IdGenType.EmployeePrivateKey);
    }

    // Optional: controlled internal access (service-layer use)
    internal void RotatePrivateKeyInternal()
    {
        RotatePrivateKey();
    }
}
