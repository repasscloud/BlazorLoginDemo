using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.Organization;

namespace Cinturon360.Application.Features.Organizations.Commands;

public sealed class CreateOrganisationCommandHandler(
    IOrganisationRepository orgRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrganisationCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateOrganisationCommand cmd, CancellationToken ct)
    {
        // Validate slug uniqueness
        var existing = await orgRepository.GetBySlugAsync(cmd.Slug, ct);
        if (existing is not null)
            return Result.Failure<string>(OrganisationErrors.SlugAlreadyInUse);

        // If parent specified, verify it exists
        if (cmd.ParentOrgId is not null)
        {
            var parent = await orgRepository.GetByIdAsync(cmd.ParentOrgId, ct);
            if (parent is null)
                return Result.Failure<string>(OrganisationErrors.ParentNotFound);
        }

        var org = Organisation.Create(
            id: IdGenerator.NewOrgId(),
            name: cmd.Name,
            slug: cmd.Slug,
            orgType: cmd.OrgType,
            parentOrgId: cmd.ParentOrgId,
            primaryEmail: cmd.PrimaryEmail,
            languageCode: cmd.LanguageCode,
            timeZone: cmd.TimeZone,
            currencyCode: cmd.CurrencyCode);

        await orgRepository.AddAsync(org, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(org.Id);
    }
}
