# RefitMultipartPoc

This repository contains a small example demonstrating:

- An ASP.NET minimal API (`RefitMultipartPoc.Api`) that accepts a multipart/form-data POST with a JSON metadata part and a file part.
- A client library (`RefitMultipartPoc.Client`) that exposes a `IApiClient` Refit interface and an `AddApiClient` extension to register an authenticated Refit client.
- A console Uploader app (`RefitMultipartPoc.Uploader`) that uploads a sample PDF to the API using the client library.

Quick start (Aspire)

This repository includes an `AppHost` which wires Keycloak, the API and the Uploader together via the Aspire developer tooling. The recommended way to run the full scenario (Keycloak + API + Uploader) is to start the `AppHost` and then launch the Uploader from the Aspire Dashboard.

1. Start the AppHost

```bash
dotnet run --project src/RefitMultipartPoc.AppHost
```

This will bring up the Aspire dashboard and start the configured services. In run mode Aspire may start Keycloak and the API and will expose a local dashboard you can use to manage and run services.

1. Open the Aspire Dashboard

When the AppHost is running the Aspire dashboard should open in your browser automatically (or visit the URL printed by the AppHost). The dashboard shows the services (keycloak, api, uploader) defined in `AppHost.cs`.

1. Configure the Uploader in the dashboard (optional)

The Uploader reads authentication values from its `appsettings.json` file under `src/RefitMultipartPoc.Uploader`. By default the sample appsettings point at the local Keycloak started by the AppHost. If you need to customize the client secret or other values, edit `src/RefitMultipartPoc.Uploader/appsettings.json` before starting the AppHost, or use environment variables in the dashboard when launching the Uploader.

Example (already configured for local Aspire Keycloak):

```json
{
  "Authentication": {
    "Authority": "http://localhost:8080/auth/realms/Sample",
    "ClientId": "app-uploader",
    "ClientSecret": "aZqoRX1Lm4mqlfpQbx23eKLCFGOIOhos",
    "Scope": "sample_api",
    "RequireHttpsMetadata": false
  },
  "Api": {
    "BaseUrl": "https://localhost:5001"
  }
}
```

1. Run the Uploader from the dashboard

Use the Aspire Dashboard to start the `uploader` service. Running it from the dashboard ensures service discovery (the uploader will discover the API URL exposed by Aspire) and will show logs in the dashboard console. The uploader performs a single upload of the embedded sample PDF and then stops.

Notes and debugging tips

- Logs: the dashboard and the individual service consoles show logs. The client token acquisition logs now include the token endpoint request (with the secret masked) and the token response to help debug 401s.
- Sensitive data: the example appsettings file contains a client secret for convenience in local dev. Do not check secrets into source control for real projects — prefer environment variables or user-secrets.
- If you prefer to run components manually, you can still run the Api and Uploader independently:

```bash
# Run API
dotnet run --project src/RefitMultipartPoc.Api

# Run Uploader
dotnet run --project src/RefitMultipartPoc.Uploader
```

The Uploader calls `AddApiClient(...)` with values from its configuration. The client library uses Refit and, when authentication is configured, Refit's AuthorizationHeaderValueGetter to attach an access token acquired via client credentials.
