namespace RefitMultipartPoc.Client;

public record UploadRespnseDto
{
    public required Guid FileId { get; set; }
}