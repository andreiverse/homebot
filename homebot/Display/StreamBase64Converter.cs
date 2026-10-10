using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeBot.Display;

public class StreamBase64Converter : JsonConverter<Stream>
{
    public override Stream? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var base64 = reader.GetString();
        if (string.IsNullOrEmpty(base64)) return null;

        var prefix = "data:image/png;base64,";
        if (base64.StartsWith(prefix))
            base64 = base64.Substring(prefix.Length);

        return new MemoryStream(Convert.FromBase64String(base64));
    }

    public override void Write(Utf8JsonWriter writer, Stream value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        byte[] bytes;
        if (value is MemoryStream ms)
        {
            bytes = ms.ToArray();
        }
        else
        {
            using var copyMs = new MemoryStream();
            if (value.CanSeek)
            {
                var pos = value.Position;
                value.Position = 0;
                value.CopyTo(copyMs);
                value.Position = pos;
            }
            else
            {
                value.CopyTo(copyMs);
            }
            bytes = copyMs.ToArray();
        }

        writer.WriteStringValue($"data:image/png;base64,{Convert.ToBase64String(bytes)}");
    }
}
