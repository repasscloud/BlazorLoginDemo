using Microsoft.AspNetCore.Localization;

public class ClaimRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var v = httpContext.User?.FindFirst("preferred_culture")?.Value;
        return Task.FromResult(!string.IsNullOrWhiteSpace(v) ? new ProviderCultureResult(v, v) : null);
    }
}