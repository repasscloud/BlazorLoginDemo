using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>Postal address for a user (Home, Billing, Mailing).</summary>
public sealed class UserAddress : Entity
{
    public string UserId { get; private set; } = string.Empty;
    public string AddressType { get; private set; } = string.Empty;  // Home, Billing, Mailing
    public string Line1 { get; private set; } = string.Empty;
    public string? Line2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string? StateProvince { get; private set; }
    public string PostalCode { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }

    private UserAddress() { }

    public static UserAddress Create(
        string id,
        string userId,
        string addressType,
        string line1,
        string city,
        string postalCode,
        string countryCode,
        string? line2 = null,
        string? stateProvince = null,
        bool isDefault = false)
        => new()
        {
            Id = id,
            UserId = userId,
            AddressType = addressType,
            Line1 = line1,
            Line2 = line2,
            City = city,
            StateProvince = stateProvince,
            PostalCode = postalCode,
            CountryCode = countryCode,
            IsDefault = isDefault,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Update(string line1, string? line2, string city, string? stateProvince, string postalCode, string countryCode)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        StateProvince = stateProvince;
        PostalCode = postalCode;
        CountryCode = countryCode;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
