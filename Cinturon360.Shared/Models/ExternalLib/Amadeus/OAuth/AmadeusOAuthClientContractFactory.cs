namespace Cinturon360.Shared.Models.ExternalLib.Amadeus;

public static class AmadeusOAuthClientContractFactory
{
    public static AmadeusOAuthClientContract FromAccount(AmadeusAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        return new AmadeusOAuthClientContract
        {
            ClientId = account.ClientId,
            ClientSecret = account.ClientSecret,
            Url = account.Url
        };
    }
}
