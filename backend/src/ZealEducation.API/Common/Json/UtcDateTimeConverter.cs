using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZealEducation.API.Common.Json;

// Treats DateTime as UTC end-to-end.
// SQL Server datetime2 strips Kind; EF returns Unspecified, and System.Text.Json
// would emit ISO without "Z" — causing the client to interpret it as local time.
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    private const string IsoUtcFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => AsUtc(reader.GetDateTime());

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(AsUtc(value).ToString(IsoUtcFormat));

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };
}

public class NullableUtcDateTimeConverter : JsonConverter<DateTime?>
{
    private const string IsoUtcFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ";

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        return AsUtc(reader.GetDateTime());
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStringValue(AsUtc(value.Value).ToString(IsoUtcFormat));
    }

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };
}
