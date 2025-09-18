using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Models.Kernel.Platform;

namespace BlazorLoginDemo.Shared.Models.Policies;

// ------------------------------
// EXPENSE POLICY (first pass)
// ------------------------------
public sealed class ExpensePolicy
{
    [Key]
    public string Id { get; set; } = NanoidDotNet.Nanoid.Generate();

    [Required, MaxLength(128)]
    public required string Name { get; set; }

    [MaxLength(512)]
    public string? Description { get; set; }

    // Currency that expense rules default to (reimbursement currency)
    [MaxLength(3)]
    public string? DefaultCurrency { get; set; }

    // Approval chain (simple static version to start; later you can move to workflow engine)
    // e.g., "Manager > Finance > Admin" as a serialized token list
    public string? ApprovalFlow { get; set; }

    // Limits / controls (starter set; expand later)
    public decimal? DailyMealCap { get; set; }
    public decimal? DailyLodgingCap { get; set; }
    public decimal? GroundTransportCap { get; set; }
    public decimal? MiscCap { get; set; }

    public bool RequireReceiptsAboveThreshold { get; set; } = true;
    public decimal? ReceiptThreshold { get; set; }

    // Category toggles (example)
    public bool AllowAlcohol { get; set; } = false;
    public bool AllowTips { get; set; } = true;

    // Policy window
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveFromUtc { get; set; }
    public DateTime? ExpiresOnUtc { get; set; }

    // Owning org
    [Required]
    public string OrganizationUnifiedId { get; set; } = default!;
    public OrganizationUnified Organization { get; set; } = default!;
}