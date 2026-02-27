using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cinturon360.Shared.Helpers;

public sealed class FlexibleEnumConverter<TEnum> : JsonConverter<TEnum?>
    where TEnum : struct, Enum
{
    public override TEnum? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();

            if (string.IsNullOrWhiteSpace(s))
                return null;

            if (Enum.TryParse<TEnum>(s, ignoreCase: true, out var result))
                return result;

            throw new JsonException($"Invalid {typeof(TEnum).Name} value '{s}'.");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var intValue = reader.GetInt32();

            if (Enum.IsDefined(typeof(TEnum), intValue))
                return (TEnum)Enum.ToObject(typeof(TEnum), intValue);

            throw new JsonException(
                $"Invalid numeric {typeof(TEnum).Name} value '{intValue}'.");
        }

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        throw new JsonException(
            $"Unexpected token {reader.TokenType} for {typeof(TEnum).Name}.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        TEnum? value,
        JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(Convert.ToInt32(value.Value));
        else
            writer.WriteNullValue();
    }
}
