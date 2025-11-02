using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RefitMultipartPoc.Client;
using RefitMultipartPoc.Uploader;

var builder = Host.CreateApplicationBuilder();

builder.AddServiceDefaults();
var cfg = builder.Configuration;

builder.Services.AddApiClient(opts =>
{
    opts.BaseUrl = cfg["Api:BaseUrl"] ?? "https://api";
    opts.Authority = cfg["Authentication:Authority"];
    opts.ClientId = cfg["Authentication:ClientId"];
    opts.ClientSecret = cfg["Authentication:ClientSecret"];
    opts.Scope = cfg["Authentication:Scope"] ?? "sample_api";
});
builder.Services.AddHostedService<UploaderWorker>();

var host = builder.Build();

await host.RunAsync();
