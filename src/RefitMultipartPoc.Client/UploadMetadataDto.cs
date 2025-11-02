using System.Text.Json.Serialization;

namespace RefitMultipartPoc.Client;

public sealed class UploadMetadataDto
{
    [JsonPropertyName("data_id")]
    public int Id { get; set; }

    [JsonPropertyName("data_name")]
    public string? Name { get; set; }
}
