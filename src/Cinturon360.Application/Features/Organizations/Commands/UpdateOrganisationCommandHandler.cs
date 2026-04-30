using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Organizations.Commands;

public sealed class UpdateOrganisationCommandHandler(
    IOrganisationRepository orgRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateOrganisationCommand, Result>
{
    public async Task<Result> Handle(UpdateOrganisationCommand cmd, CancellationToken ct)
    {
        var org = await orgRepository.GetByIdAsync(cmd.OrgId, ct);
        if (org is null)
            return Result.Failure(OrganisationErrors.NotFound);

        org.UpdateDetails(cmd.Name, cmd.PrimaryEmail, cmd.PrimaryPhone, cmd.Website);
        org.UpdateLocale(cmd.LanguageCode, cmd.TimeZone, cmd.CurrencyCode);
        org.SetSupportTicketEmailTemplateCode(cmd.SupportTicketEmailTemplateCode);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
