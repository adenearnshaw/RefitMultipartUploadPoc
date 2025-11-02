using System.Text.Json;
using System.Text.Json.Serialization;

namespace RefitMultipartPoc.Api;

public sealed class UploadMetadata : IParsable<UploadMetadata>
{
    [JsonPropertyName("data_mime_type")]
    public string? MimeType { get; set; }

    [JsonPropertyName("data_name")]
    public string? Name { get; set; }

    // Simple parser that expects the input to be a JSON object matching the shape
    public static UploadMetadata Parse(string s, IFormatProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new FormatException("Input string was null or whitespace.");

        try
        {
            var doc = JsonSerializer.Deserialize<UploadMetadata>(s);
            if (doc is null)
                throw new FormatException("Unable to deserialize UploadMetadata from input.");
            return doc;
        }
        catch (JsonException ex)
        {
            throw new FormatException("Invalid JSON for UploadMetadata", ex);
        }
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out UploadMetadata result)
    {
        result = null!;
        if (string.IsNullOrWhiteSpace(s))
            return false;

        try
        {
            var doc = JsonSerializer.Deserialize<UploadMetadata>(s);
            if (doc is null)
                return false;
            result = doc;
            return true;
        }
        catch
        {
            return false;
        }
    }
}