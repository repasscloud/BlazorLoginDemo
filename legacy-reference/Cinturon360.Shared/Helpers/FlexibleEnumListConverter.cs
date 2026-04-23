using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cinturon360.Shared.Helpers;

public sealed class FlexibleEnumListConverter<TEnum>
    : JsonConverter<List<TEnum>>
    where TEnum : struct, Enum
{
    public override List<TEnum>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException($"Expected array for {typeof(TEnum).Name} list.");

        var list = new List<TEnum>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return list;

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();

                if (!Enum.TryParse<TEnum>(s, ignoreCase: true, out var value))
                    throw new JsonException($"Invalid {typeof(TEnum).Name} value '{s}'.");

                list.Add(value);
                continue;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                var i = reader.GetInt32();

                if (!Enum.IsDefined(typeof(TEnum), i))
                    throw new JsonException($"Invalid numeric {typeof(TEnum).Name} value '{i}'.");

                list.Add((TEnum)Enum.ToObject(typeof(TEnum), i));
                continue;
            }

            throw new JsonException(
                $"Unexpected token {reader.TokenType} in {typeof(TEnum).Name} list.");
        }

        throw new JsonException("Unexpected end of JSON while reading enum list.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        List<TEnum> value,
        JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var item in value)
            writer.WriteNumberValue(Convert.ToInt32(item));

        writer.WriteEndArray();
    }
}
