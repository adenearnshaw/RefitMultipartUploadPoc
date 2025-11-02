using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RefitMultipartPoc.Client;

namespace RefitMultipartPoc.Uploader;

public class UploaderWorker(IApiClient api, IHostApplicationLifetime lifetime, ILogger<UploaderWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var assembly = typeof(UploaderWorker).Assembly;
            var resourceName = Array.Find(assembly.GetManifestResourceNames(), n => n.EndsWith("Sample PDF Upload.pdf", StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
            {
                logger.LogError("Embedded resource 'Sample PDF Upload.pdf' not found in assembly.");
                lifetime.StopApplication();
                return;
            }

            await using var resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
            {
                logger.LogError("Failed to open embedded resource stream: {name}", resourceName);
                lifetime.StopApplication();
                return;
            }

            using var ms = new MemoryStream();
            await resourceStream.CopyToAsync(ms, stoppingToken);
            ms.Position = 0;
            var streamPart = new Refit.StreamPart(ms, "Sample PDF Upload.pdf", "application/pdf");

            var meta = new UploadMetadataDto { Id = 1, Name = "UploaderWorker" };

            logger.LogInformation("Uploading embedded resource {resource}", resourceName);

            var resp = await api.UploadAsync(meta, streamPart);

            if (!resp.IsSuccessStatusCode)
            {
                logger.LogError("Upload failed with status {status}: {error}", resp.StatusCode, resp.Error);
            }
            else
            {
                logger.LogInformation("Upload succeeded. File ID: {fileId}", resp.Content?.FileId);
                logger.LogInformation("Upload finished with status {status}", resp.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Upload failed");
        }
        finally
        {
            // Stop the host after the upload completes
            lifetime.StopApplication();
        }
    }
}