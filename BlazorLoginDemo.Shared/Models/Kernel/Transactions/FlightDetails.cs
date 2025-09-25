namespace BlazorLoginDemo.Shared.Models.Kernel.Transactions;

public class FlightDetails
{
    public Guid FlightID { get; set; }
    public Guid ItemID { get; set; }

    public string CarrierCode { get; set; } = null!;
    public string FlightNumber { get; set; } = null!;

    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }

    public string DepartureAirport { get; set; } = null!;
    public string ArrivalAirport { get; set; } = null!;
    public string CabinClass { get; set; } = "Economy";
    public string? TicketNumber { get; set; }

    // Navigation
    public TransactionItem TransactionItem { get; set; } = null!;
}
