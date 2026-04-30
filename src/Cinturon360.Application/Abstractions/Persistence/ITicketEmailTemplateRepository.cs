using Cinturon360.Domain.Entities.Ticketing;

namespace Cinturon360.Application.Abstractions.Persistence;

public interface ITicketEmailTemplateRepository
{
    Task<TicketEmailTemplate?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<TicketEmailTemplate?> GetByCodeAndLanguageAsync(string code, string languageCode, CancellationToken ct = default);
    Task<IReadOnlyList<TicketEmailTemplate>> ListAsync(string? code = null, CancellationToken ct = default);
    Task AddAsync(TicketEmailTemplate template, CancellationToken ct = default);
    void Remove(TicketEmailTemplate template);
}