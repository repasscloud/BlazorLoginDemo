namespace BlazorLoginDemo.Models.Policies;

// Join entity for disabled countries in a travel policy
public class TravelPolicyDisabledCountry {
    public required string TravelPolicyId { get; set; }
    public TravelPolicy? TravelPolicy { get; set; }
    
    public int CountryId { get; set; }
    public Country? Country { get; set; }
}
