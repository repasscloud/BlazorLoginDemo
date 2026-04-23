namespace Cinturon360.Common.Results;

/// <summary>
/// Represents an application error with a code and description.
/// </summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    public static Error NotFound(string entityName, object id) =>
        new($"{entityName}.NotFound", $"{entityName} with id '{id}' was not found.");

    public static Error Validation(string field, string message) =>
        new($"Validation.{field}", message);

    public static Error Unauthorized(string message = "Unauthorized access.") =>
        new("Error.Unauthorized", message);

    public static Error Conflict(string message) =>
        new("Error.Conflict", message);

    public static Error Unexpected(string message = "An unexpected error occurred.") =>
        new("Error.Unexpected", message);
}
