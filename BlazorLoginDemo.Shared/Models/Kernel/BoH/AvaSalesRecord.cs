using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Models.Kernel.Client;
using BlazorLoginDemo.Shared.Validation;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Models.Kernel.BoH;

public class AvaSalesRecord
{
    [Key]
    public string Id { get; set; } = Nanoid.Generate();
    
    [Required]
    public required string AvaEmployeeRecordId { get; set; }

    [Required]  
    public required string SalesPerson { get; set; }

    public string? LicenseId { get; set; }  // when a license is created, it should be linked back to this record (API will handle)

    public string? ClientId { get; set; }  // when a license is created, it should be linked back to this record (API will handle)

    public required AvaClientLicense License { get; set; }

    [Required]
    [PaymentMethodValidation]
    public required string PaymentMethod { get; set; }

    [Required]
    public required string PaymentReference { get; set; }

    [Required]
    public DateTime SalesDate { get; set; } = DateTime.UtcNow;
}
