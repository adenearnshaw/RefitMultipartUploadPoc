using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("kc-username", "admin");
var password = builder.AddParameter("kc-password", "admin", secret: true);

var keycloak = builder.AddKeycloak("keycloak",
                                port: 8080,
                                adminUsername: username,
                                adminPassword: password)
    .WithDataVolume()
    .WithRealmImport("./Keycloak/Realms");

var api = builder.AddProject<Projects.RefitMultipartPoc_Api>("api")
    .WithReference(keycloak)
    .WaitFor(keycloak);

var uploader = builder.AddProject<Projects.RefitMultipartPoc_Uploader>("uploader")
    .WithReference(api)
    .WithReference(keycloak)
    .WithExplicitStart();

var scalar = builder.AddScalarApiReference(options =>
{
    options
        .PreferHttpsEndpoint() // Use HTTPS endpoints when available
        .AllowSelfSignedCertificates(); // Trust self-signed certificates
})
.WaitFor(api);

scalar
    .WithApiReference(api);

builder.Build().Run();
