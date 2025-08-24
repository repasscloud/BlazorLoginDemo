using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlazorLoginDemo.Shared.Models.Kernel.Client;
using BlazorLoginDemo.Shared.Validation;
using Microsoft.EntityFrameworkCore;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Models.User;

[Index(nameof(AspNetUsersId), IsUnique = true)]  // enforce unique constraint
[Index(nameof(Email), IsUnique = true)]
public class AvaUser
{
    [Key]
    public string Id { get; set; } = Nanoid.Generate();

    [Required]  // make this required for true 1:1
    public string AspNetUsersId { get; set; } = default!;

    public bool IsActive { get; set; } = true;

    [Required]
    [PassportNameValidation]
    public required string FirstName { get; set; }
    
    [PassportNameValidation]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MiddleName { get; set; }

    [Required]
    [PassportNameValidation]
    public required string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    // user location defaults
    [AlphaNumeric3Validation]
    public string? OriginLocationCode { get; set; }

    // flight default details
    [CabinTypeValidation]
    public string DefaultFlightSeating { get; set; } = "ECONOMY";

    [CabinTypeValidation]
    public string MaxFlightSeating { get; set; } = "ECONOMY";
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IncludedAirlineCodes { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExcludedAirlineCodes { get; set; }
    public bool NonStopFlight { get; set; } = false;
    
    // financial considerations for bookings
    [CurrencyTypeValidation]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency must be exactly 3 uppercase letters.")]
    [JsonPropertyName("currency")]
    [DefaultValue("AUD")]
    public string DefaultCurrencyCode { get; set; } = "AUD";
    public int MaxFlightPrice { get; set; } = 0;

    // travel policy - if this is not provided, it will find out if one should exist from
    // the AvaClientId (if provided) else from the email address (if domain exists)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TravelPolicyId { get; set; }

    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonIgnore]
    public TravelPolicy? TravelPolicy { get; set; }

    // Optional link to a Client; not every user must have a Client parent, this will also be
    // updated by the API if it finds a match for the email address domain
    public string? AvaClientId { get; set; }

    [JsonIgnore]
    public AvaClient? AvaClient { get; set; }

    // this will only be filled in by the API, it's completely optional for the user to provide etc
    public string? ClientId { get; set; }
}
