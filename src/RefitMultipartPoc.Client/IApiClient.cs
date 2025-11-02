using Refit;

namespace RefitMultipartPoc.Client;

public interface IApiClient
{
    // Refit v7 supports multipart. We accept a complex POCO part named "data" and a file part named "file".
    [Multipart]
    [Post("/upload")]
    Task<ApiResponse<UploadRespnseDto>> UploadAsync(
        [AliasAs("data")] UploadMetadataDto data,
        [AliasAs("file")] StreamPart file);
}