using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Organizations.Commands;

public static class OrganisationErrors
{
    public static readonly Error NotFound          = new("organisation.not_found", "Organisation not found.");
    public static readonly Error SlugAlreadyInUse  = new("organisation.slug_in_use", "A slug with this value is already in use.");
    public static readonly Error ParentNotFound    = new("organisation.parent_not_found", "Parent organisation not found.");
    public static readonly Error CannotDeleteWithChildren = new("organisation.has_children", "Cannot delete an organisation that has child organisations.");
}
