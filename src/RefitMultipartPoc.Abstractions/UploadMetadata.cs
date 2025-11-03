using System.Text.Json;
using System.Text.Json.Serialization;

namespace RefitMultipartPoc.Abstractions;

public sealed class UploadMetadata : IParsable<UploadMetadata>
{
    [JsonPropertyName("sourceId")]
    public required string SourceId { get; init; }
    
    [JsonPropertyName("fileName")]
    public required string FileName { get; init; }

    [JsonPropertyName("mimeType")]
    public required string MimeType { get; init; }

    [JsonPropertyName("containsPI")]
    public bool ContainsPI { get; init; }

    [JsonPropertyName("tags")]
    public Dictionary<string, string> Tags { get; init; } = [];

#region IParsable implementation
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
#endregion
}