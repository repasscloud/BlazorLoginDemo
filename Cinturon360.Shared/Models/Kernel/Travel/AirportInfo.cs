using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Models.Static;

namespace Cinturon360.Shared.Models.Kernel.Travel;

public class AirportInfo
{
    [Key]
    public int Id { get; set; }

    public string Ident { get; set; } = default!;

    [DefaultValue(AirportType.Unknown)]
    public AirportType Type { get; set; } = AirportType.Unknown;
    public string Name { get; set; } = default!;
    public double LatitudeDeg { get; set; }
    public double LongitudeDeg { get; set; }
    public int ElevationFt { get; set; }

    [DefaultValue(AirportContinent.Unknown)]
    public AirportContinent Continent { get; set; } = AirportContinent.Unknown;

    public Iso3166_Alpha2 IsoCountry { get; set; }
    public string IsoRegion { get; set; } = default!;
    public string Municipality { get; set; } = default!;
    public bool ScheduledService { get; set; } = false;
    public string? GpsCode { get; set; }
    public string? IataCode { get; set; }
    public string? LocalCode { get; set; }   
}





// "id","ident","type","name","latitude_deg","longitude_deg","elevation_ft","continent","iso_country","iso_region","municipality","scheduled_service","gps_code","iata_code","local_code","home_link","wikipedia_link","keywords"
