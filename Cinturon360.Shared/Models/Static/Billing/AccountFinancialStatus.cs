namespace Cinturon360.Shared.Models.Static.Billing;
public enum AccountFinancialStatus
{
    /// <summary>
    /// Account is pending activation or configuration.
    /// </summary>
    Pending,
    
    /// <summary>
    /// Account is fully up to date, no unpaid invoices.
    /// </summary>
    Current,

    /// <summary>
    /// Within allowed billing period (i.e., still inside Net30/Net60 terms but unpaid).
    /// </summary>
    InBillingPeriod,

    /// <summary>
    /// Past the due date; invoice is overdue.
    /// </summary>
    Overdue,

    /// <summary>
    /// Account has been placed on hold due to non-payment (manual or automatic).
    /// </summary>
    Suspended,

    /// <summary>
    /// Account is formally terminated (requires reactivation/new agreement).
    /// </summary>
    Terminated
}



// Logic to use this enum later
// if (invoice.IsPaid)
//     status = FinancialAccountStatus.Current;
// else if (DateTime.UtcNow <= invoice.DueDate)
//     status = FinancialAccountStatus.InBillingPeriod;
// else if (DateTime.UtcNow > invoice.DueDate && !account.IsSuspended)
//     status = FinancialAccountStatus.Overdue;
// else if (account.IsSuspended)
//     status = FinancialAccountStatus.Suspended;
// else if (account.IsTerminated)
//     status = FinancialAccountStatus.Terminated;
