using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Models.Static.Billing;
using BlazorLoginDemo.Shared.Validation;

namespace BlazorLoginDemo.Shared.Models.Kernel.Transactions;

public class TransactionItem
{
    public Guid ItemID { get; set; }
    public Guid TransactionID { get; set; }

    public TransactionItemType ItemType { get; set; } = TransactionItemType.Flight;
    public Guid ItemRefID { get; set; } // Refers to the domain-specific item like FlightDetails

    public decimal Amount { get; set; }
    
    [Required]
    [StringLength(3)]
    [CurrencyTypeValidation]
    public required string Currency { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public Transaction Transaction { get; set; } = null!;
    public FlightDetails? FlightDetails { get; set; } // Nullable, only set if ItemType == "Flight"
}
