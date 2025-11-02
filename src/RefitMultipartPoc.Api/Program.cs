using RefitMultipartPoc.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.AddServiceDefaults();

// Add HTTP logging to capture request/response headers and bodies for debugging.
builder.Services.AddHttpLogging(logging =>
{
    // Log request and response headers and body. Be cautious with sensitive data.
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPropertiesAndHeaders
                          | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestBody
                          | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseBody
                          | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;

    // Limit body size to avoid excessive memory usage in logs (adjust as needed).
    logging.RequestBodyLogLimit = 4096; // bytes
    logging.ResponseBodyLogLimit = 4096; // bytes

    // Optionally filter which headers to log; by default, some sensitive headers are redacted.
    // logging.Headers.Clear();
});

// Configure JWT Bearer authentication from appsettings
var authSection = builder.Configuration.GetSection("Authentication");
var authority = authSection.GetValue<string>("Authority");
var audience = authSection.GetValue<string>("Audience");

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = authority;
        options.Audience = audience;
        // For local testing with self-signed or http, allow insecure (not recommended for production)
        options.RequireHttpsMetadata = false;
    });

// Require authenticated user by default
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use HTTP logging early in the pipeline so it captures authentication headers etc.
app.UseHttpLogging();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();

// Minimal endpoint to accept multipart/form-data with a JSON string part named "data"
// and a file part named "file". This endpoint does not persist or process the parts;
// it only reads them to allow testing multipart uploads.
app.MapPost("/upload", ([AsParameters] UploadRequest request) =>
{
    // Model binding will attempt to parse the "data" form field into UploadMetadata using IParsable<T>.
    return Results.Ok(new { HasData = request.Data != null, FileName = request.File?.FileName, FileId = Guid.NewGuid() });
})
.DisableAntiforgery()
.Accepts<UploadMetadata>("multipart/form-data")
.Accepts<IFormFile>("multipart/form-data")
.WithName("UploadDocument");

app.Run();
