using Cinturon360.Shared.Models.ExternalLib.Amadeus;
using Cinturon360.Shared.Models.Kernel.UI.Amadeus;

namespace Cinturon360.Shared.Mappers;

public static class AmadeusAccountMapper
{
    public static AmadeusAccount ToEntity(AmadeusAccountEditModel model)
    {
        return new AmadeusAccount
        {
            TmcId = model.TmcId,
            DisplayName = model.DisplayName,
            ClientId = model.ClientId,
            ClientSecret = model.ClientSecret,
            OfficeId = model.OfficeId,
            CountryCode = model.CountryCode,
            DefaultCurrency = model.DefaultCurrency,
            TicketingEnabled = model.TicketingEnabled,
            DefaultPlatingCarrier = model.DefaultPlatingCarrier,
            TicketPrefix = model.TicketPrefix,
            Url = new AmadeusUrlSettings
            {
                ApiEndpoint = model.ApiEndpoint
            }
        };
    }

    public static AmadeusAccountEditModel ToEditModel(AmadeusAccount entity)
    {
        return new AmadeusAccountEditModel
        {
            TmcId = entity.TmcId,
            DisplayName = entity.DisplayName,
            ClientId = entity.ClientId,
            ClientSecret = entity.ClientSecret,
            OfficeId = entity.OfficeId,
            CountryCode = entity.CountryCode,
            DefaultCurrency = entity.DefaultCurrency,
            TicketingEnabled = entity.TicketingEnabled,
            DefaultPlatingCarrier = entity.DefaultPlatingCarrier,
            TicketPrefix = entity.TicketPrefix,
            ApiEndpoint = entity.Url.ApiEndpoint
        };
    }
}
