using Microsoft.EntityFrameworkCore;
using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.ExternalLib.Amadeus;
using Cinturon360.Shared.Services.Interfaces.External;

namespace Cinturon360.Shared.Services.External;

public sealed class AmadeusAccountStore : IAmadeusAccountStore
{
    private readonly ApplicationDbContext _db;

    public AmadeusAccountStore(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AmadeusAccount> GetByTmcIdAsync(string tmcId)
    {
        var account = await _db.AmadeusAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.TmcId == tmcId);

        if (account is null)
            throw new InvalidOperationException(
                $"No Amadeus account configured for TMC '{tmcId}'.");

        return account;
    }
}
