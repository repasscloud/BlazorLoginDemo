using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Travellers.Commands;

public static class TravellerErrors
{
    public static readonly Error ProfileNotFound = new("Traveller.ProfileNotFound", "Traveller profile not found.");
    public static readonly Error LoyaltyProgramNotFound = new("Traveller.LoyaltyProgramNotFound", "Loyalty program not found.");
    public static readonly Error EmergencyContactNotFound = new("Traveller.EmergencyContactNotFound", "Emergency contact not found.");
    public static readonly Error AddressNotFound = new("Traveller.AddressNotFound", "Address not found.");
}
