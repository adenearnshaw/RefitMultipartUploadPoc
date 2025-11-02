namespace RefitMultipartPoc.Client;

/// <summary>
/// Options used to request tokens via client credentials.
/// </summary>
internal sealed record ClientCredentialsOptions(string Authority, string ClientId, string ClientSecret, string Scope);
