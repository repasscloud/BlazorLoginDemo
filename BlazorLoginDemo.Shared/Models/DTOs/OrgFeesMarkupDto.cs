using BlazorLoginDemo.Shared.Models.Static.Billing;
namespace BlazorLoginDemo.Shared.Models.DTOs;
public sealed class OrgFeesMarkupDto
{
    public decimal PnrCreationFee { get; set; }
    public decimal PnrChangeFee { get; set; }

    public decimal FlightMarkupPercent { get; set; }
    public decimal FlightPerItemFee { get; set; }
    public ServiceFeeType FlightFeeType { get; set; }

    public decimal HotelMarkupPercent { get; set; }
    public decimal HotelPerItemFee { get; set; }
    public ServiceFeeType HotelFeeType { get; set; }

    public decimal CarMarkupPercent { get; set; }
    public decimal CarPerItemFee { get; set; }
    public ServiceFeeType CarFeeType { get; set; }

    public decimal RailMarkupPercent { get; set; }
    public decimal RailPerItemFee { get; set; }
    public ServiceFeeType RailFeeType { get; set; }

    public decimal TransferMarkupPercent { get; set; }
    public decimal TransferPerItemFee { get; set; }
    public ServiceFeeType TransferFeeType { get; set; }

    public decimal ActivityMarkupPercent { get; set; }
    public decimal ActivityPerItemFee { get; set; }
    public ServiceFeeType ActivityFeeType { get; set; }

    public decimal TravelMarkupPercent { get; set; }
    public decimal TravelPerItemFee { get; set; }
    public ServiceFeeType TravelFeeType { get; set; }
}