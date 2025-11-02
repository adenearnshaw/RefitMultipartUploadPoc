using System.Net.Http.Headers;

namespace RefitMultipartPoc.Client;

/// <summary>
/// Delegating handler that injects a bearer token using the ClientCredentialsTokenProvider.
/// </summary>
internal sealed class AuthTokenHandler(ClientCredentialsTokenProvider provider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await provider.GetTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
