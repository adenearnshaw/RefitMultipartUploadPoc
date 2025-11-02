using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace RefitMultipartPoc.Client;

public static partial class ApiClientServiceCollectionExtensions
{
    /// <summary>
    /// Consolidated AddApiClient which accepts a ClientOptions configuration action.
    /// Use ClientOptions to supply base URL, optional client credentials for token acquisition,
    /// and an optional HttpClient configuration delegate.
    /// </summary>
    public static IServiceCollection AddApiClient(this IServiceCollection services, Action<ApiClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var opts = new ApiClientOptions();
        configure(opts);

        if (string.IsNullOrWhiteSpace(opts.BaseUrl))
            throw new ArgumentException("BaseUrl must be provided in ClientOptions.", nameof(opts.BaseUrl));

        // If authority/client credentials provided, set up token support
        var hasClientCredentials = !string.IsNullOrWhiteSpace(opts.Authority)
                                   && !string.IsNullOrWhiteSpace(opts.ClientId)
                                   && !string.IsNullOrWhiteSpace(opts.ClientSecret);

        if (!hasClientCredentials)
        {
            throw new ArgumentException("Authority, ClientId, and ClientSecret must all be provided to use client credentials.", nameof(opts.Authority));
        }
        
        var tokenBase = opts.Authority!.TrimEnd('/');
        services.AddHttpClient("token", c => c.BaseAddress = new Uri(tokenBase));

        services.AddSingleton(new ClientCredentialsOptions(opts.Authority!, opts.ClientId!, opts.ClientSecret!, opts.Scope ?? "sample_api"));
        services.AddSingleton<ClientCredentialsTokenProvider>();
        services.AddSingleton<AuthTokenHandler>();

        // Configure the named API client. Refit will attach Authorization via
        // AuthorizationHeaderValueGetter, so we do not add an HTTP message handler here.
        services.AddHttpClient("api", client =>
        {
            client.BaseAddress = new Uri(opts.BaseUrl);
        })
        .AddHttpMessageHandler<AuthTokenHandler>();


        // Register IApiClient using named client
        services.AddTransient<IApiClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var http = factory.CreateClient("api");


            var provider = sp.GetRequiredService<ClientCredentialsTokenProvider>();
            var settings = new RefitSettings
            {
                // Refit expects a Func<HttpRequestMessage, CancellationToken, Task<string>>
                AuthorizationHeaderValueGetter = async (request, ct) =>
                {
                    var token = await provider.GetTokenAsync(ct);
                    return string.IsNullOrWhiteSpace(token) ? string.Empty : $"Bearer {token}";
                }
            };

            return RestService.For<IApiClient>(http, settings);
        });

        return services;
    }
}
