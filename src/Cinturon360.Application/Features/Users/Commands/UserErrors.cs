using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Users.Commands;

public static class UserErrors
{
    public static readonly Error EmailAlreadyInUse = new("user.email_in_use",   "A user with this email already exists.");
    public static readonly Error NotFound          = new("user.not_found",      "User not found.");
    public static readonly Error CannotSelfDelete  = new("user.self_delete",    "Cannot delete your own account.");
}
