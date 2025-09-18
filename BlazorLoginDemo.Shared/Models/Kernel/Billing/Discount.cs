namespace BlazorLoginDemo.Shared.Models.Kernel.Billing;

public class Discount
{
    public decimal PercentOff { get; set; }
    public DateTime ValidUntil { get; set; }

    public bool IsActive => DateTime.UtcNow <= ValidUntil;
}