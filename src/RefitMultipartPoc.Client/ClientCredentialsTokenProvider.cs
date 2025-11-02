using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace RefitMultipartPoc.Client;

/// <summary>
/// Simple client credentials token provider that caches a token until expiration.
/// Not intended as a production-grade token cache; suitable for samples and tests.
/// </summary>

internal sealed class ClientCredentialsTokenProvider(
    IHttpClientFactory factory,
    ClientCredentialsOptions options,
    ILogger<ClientCredentialsTokenProvider> logger)
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting access token for client_id={clientId} scope={scope}", options.ClientId, options.Scope);

        if (!string.IsNullOrEmpty(_accessToken) && DateTimeOffset.UtcNow < _expiresAt)
            return _accessToken;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTimeOffset.UtcNow < _expiresAt)
                return _accessToken;

            var client = factory.CreateClient("token");
            var tokenEndpoint = new Uri(new Uri(options.Authority), "protocol/openid-connect/token");
            var body = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string,string>("grant_type", "client_credentials"),
                new KeyValuePair<string,string>("client_id", options.ClientId),
                new KeyValuePair<string,string>("client_secret", options.ClientSecret),
                new KeyValuePair<string,string>("scope", options.Scope),
            });

            try
            {
                // Log the outgoing token request for debugging, but mask the client_secret
                var bodyString = await body.ReadAsStringAsync(cancellationToken);
                var masked = Regex.Replace(bodyString, "(?<=client_secret=)[^&]*", "***");
                logger.LogInformation("Requesting token from {url} with body: {body}", tokenEndpoint, masked);

                using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint) { Content = body };
                using var resp = await client.SendAsync(request, cancellationToken);

                var respBody = await resp.Content.ReadAsStringAsync(cancellationToken);
                if (!resp.IsSuccessStatusCode)
                {
                    logger.LogWarning("Token endpoint returned non-success status {status}: {body}", resp.StatusCode, respBody);
                }

                resp.EnsureSuccessStatusCode();

                using var doc = JsonDocument.Parse(respBody);
                var root = doc.RootElement;
                var token = root.GetProperty("access_token").GetString();
                var expiresIn = root.GetProperty("expires_in").GetInt32();

                _accessToken = token ?? throw new InvalidOperationException("token is null");
                _expiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 30); // renew a bit early
                logger.LogInformation("Acquired access token, expires at {when}", _expiresAt);
                return _accessToken;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to acquire token from {url}", tokenEndpoint);
                throw;
            }
        }
        finally
        {
            _lock.Release();
        }
    }
}
