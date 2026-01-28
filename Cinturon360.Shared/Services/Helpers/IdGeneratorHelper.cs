namespace Cinturon360.Shared.Services.Helpers;

public static class IDGeneratorHelper
{
    // avoids 0,1,2,5,8,B,I,L,O,S,Z for better readability
    public static readonly string Alphabet = "34679ACDEFGHJKMNPQRTUVWXY";

    public static string GenerateId(
        IdGenType type = IdGenType.Default,
        string? prefix = null)
    {
        switch (type)
        {
            case IdGenType.Default:
                // Default never uses prefix
                return NanoidDotNet.Nanoid.Generate(Alphabet, (int)type);

            case IdGenType.Quote:
                if (prefix is null)
                    throw new ArgumentNullException(
                        nameof(prefix),
                        "Prefix is required for this IdGenType.");

                if (string.IsNullOrWhiteSpace(prefix))
                    throw new ArgumentException(
                        "Prefix cannot be empty or whitespace.",
                        nameof(prefix));

                return $"{prefix}_{NanoidDotNet.Nanoid.Generate(Alphabet, (int)type)}";

            case IdGenType.AvaClient:


            default:
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "Unsupported IdGenType.");
        }
    }
}

public enum IdGenType : short
{
    Default = 14,
    Quote = 12,
    AvaClient = 21
}
