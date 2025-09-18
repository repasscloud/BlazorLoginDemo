using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Models.Static.Billing;
using BlazorLoginDemo.Shared.Validation;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Models.Kernel.Transactions;

public class Transaction
{
    [Key]
    public string TransactionID { get; set; } = Nanoid.Generate();

    [Required]
    [StringLength(14)]
    public required string InvoiceID { get; set; } = Nanoid.Generate(alphabet: Nanoid.Alphabets.HexadecimalUppercase, size: 14);
    
    [Required]
    [StringLength(10)]
    public required string AvaCliendID { get; set; }
    
    [Required]
    [StringLength(36)]
    public required Guid AvaUserID { get; set; }

    public DateTime TransactionDate { get; set; }
    public decimal TotalAmount { get; set; }
    
    [Required]
    [StringLength(3)]
    [CurrencyTypeValidation]
    public required string Currency { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public List<TransactionItem> Items { get; set; } = new();
}
