using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Travel;

/// <summary>ISO country reference data. Seeded from provider or static file.</summary>
public sealed class Country : Entity
{
    public string IsoCode2 { get; private set; } = string.Empty;
    public string IsoCode3 { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? PhonePrefix { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";
    public bool IsActive { get; private set; } = true;

    private Country() { }

    public static Country Create(string id, string iso2, string iso3, string name, string? phonePrefix, string currencyCode)
        => new()
        {
            Id = id,
            IsoCode2 = iso2.ToUpperInvariant(),
            IsoCode3 = iso3.ToUpperInvariant(),
            Name = name,
            PhonePrefix = phonePrefix,
            CurrencyCode = currencyCode,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}

/// <summary>City reference data, linked to a country.</summary>
public sealed class City : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? IataCode { get; private set; }
    public string CountryCode { get; private set; } = string.Empty;
    public string? TimeZone { get; private set; }
    public bool IsActive { get; private set; } = true;

    private City() { }

    public static City Create(string id, string name, string countryCode, string? iataCode = null, string? timeZone = null)
        => new()
        {
            Id = id,
            Name = name,
            IataCode = iataCode?.ToUpperInvariant(),
            CountryCode = countryCode.ToUpperInvariant(),
            TimeZone = timeZone,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}

/// <summary>Airport reference data, linked to a city and country.</summary>
public sealed class Airport : Entity
{
    public string IataCode { get; private set; } = string.Empty;
    public string? IcaoCode { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string CityCode { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public string? TimeZone { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Airport() { }

    public static Airport Create(
        string id,
        string iataCode,
        string name,
        string cityCode,
        string countryCode,
        string? icaoCode = null,
        string? timeZone = null,
        decimal? lat = null,
        decimal? lon = null)
        => new()
        {
            Id = id,
            IataCode = iataCode.ToUpperInvariant(),
            IcaoCode = icaoCode?.ToUpperInvariant(),
            Name = name,
            CityCode = cityCode.ToUpperInvariant(),
            CountryCode = countryCode.ToUpperInvariant(),
            TimeZone = timeZone,
            Latitude = lat,
            Longitude = lon,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}
