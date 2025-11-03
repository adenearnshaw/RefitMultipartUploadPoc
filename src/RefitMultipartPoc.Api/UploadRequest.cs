using Microsoft.AspNetCore.Mvc;
using RefitMultipartPoc.Abstractions;

namespace RefitMultipartPoc.Api;

public record UploadRequest
{
    [FromForm]
    public UploadMetadata? Data { get; set; }

    [FromForm]
    public IFormFile? File { get; set; }
}
