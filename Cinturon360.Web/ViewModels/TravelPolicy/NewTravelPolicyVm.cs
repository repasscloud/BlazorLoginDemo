using Cinturon360.Web.ViewModels.ReferenceData;

public sealed record NewTravelPolicyVm
{
    // Context
    public string Rid { get; init; } = default!;
    public string OrgId { get; init; } = default!;
    public string UserId { get; init; } = default!;

    // Reference data (UI only)
         
    public GeographyReference GeographyRef { get; init; } = default!;
    public CurrencyReference CurrencyRef { get; init; } = default!;
    public AcrissReference AcrissRef { get; init; } = default!;
    public FareTypeReference FareTypeRef { get; init; } = default!;
    public HotelRoomTypeReference HotelRoomTypeRef { get; init; } = default!;
    public RailClassReference RailClassRef { get; init; } = default!;
    public CabinClassCoverageReference CabinClassCoverageRef { get; init; } = default!;

    // UI state
    public bool IsLoading { get; set; }
    public string? Error { get; set; }
}
