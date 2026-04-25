using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Organizations.Commands;

public sealed class DeactivateOrganisationCommandHandler(
    IOrganisationRepository orgRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateOrganisationCommand, Result>
{
    public async Task<Result> Handle(DeactivateOrganisationCommand cmd, CancellationToken ct)
    {
        var org = await orgRepository.GetByIdAsync(cmd.OrgId, ct);
        if (org is null)
            return Result.Failure(OrganisationErrors.NotFound);

        org.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public sealed class ReactivateOrganisationCommandHandler(
    IOrganisationRepository orgRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ReactivateOrganisationCommand, Result>
{
    public async Task<Result> Handle(ReactivateOrganisationCommand cmd, CancellationToken ct)
    {
        var org = await orgRepository.GetByIdAsync(cmd.OrgId, ct);
        if (org is null)
            return Result.Failure(OrganisationErrors.NotFound);

        org.Reactivate();
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
