namespace Cinturon360.Shared.Models.Static;

public static class HotelRoomType
{
    public static readonly (string code, string label)[] HotelRoomOptions =
    [
        ("STD,STANDARD","Standard"),
        ("SUP,SUPERIOR","Superior"),
        ("DLX,DELUXE","Deluxe"),
        ("EXEC,EXECUTIVE","Executive"),
        ("CLUB","Club")
    ];
}
