using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Ticketing;
using Microsoft.EntityFrameworkCore;

namespace Cinturon360.Data.Repositories;

public sealed class TicketEmailTemplateRepository(AppDbContext db) : ITicketEmailTemplateRepository
{
    public Task<TicketEmailTemplate?> GetByIdAsync(string id, CancellationToken ct = default)
        => db.TicketEmailTemplates
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<TicketEmailTemplate?> GetByCodeAndLanguageAsync(string code, string languageCode, CancellationToken ct = default)
        => db.TicketEmailTemplates
            .FirstOrDefaultAsync(x => x.Code == code && x.LanguageCode == languageCode, ct);

    public async Task<IReadOnlyList<TicketEmailTemplate>> ListAsync(string? code = null, CancellationToken ct = default)
    {
        var query = db.TicketEmailTemplates.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(code))
            query = query.Where(x => x.Code == code);

        return await query
            .OrderBy(x => x.Code)
            .ThenBy(x => x.LanguageCode)
            .ToListAsync(ct);
    }

    public async Task AddAsync(TicketEmailTemplate template, CancellationToken ct = default)
        => await db.TicketEmailTemplates.AddAsync(template, ct);

    public void Remove(TicketEmailTemplate template)
        => db.TicketEmailTemplates.Remove(template);
}