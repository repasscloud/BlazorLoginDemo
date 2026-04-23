using Cinturon360.Shared.Models.Static.Travel;

namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record HotelRoomTypeOption(
    HotelRoomClassType Type,    // HotelRoomType.Standard
    string DisplayName          // "Standard Room"
);

public sealed record HotelRoomTypeReference
{
    public IReadOnlyList<HotelRoomTypeOption> Options { get; init; } = [];
}
