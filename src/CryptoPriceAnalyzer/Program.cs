using CryptoPriceAnalyzer.Data;
using CryptoPriceAnalyzer.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// load config from local.settings.json + env
builder.Configuration
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// bind provider options
builder.Services.Configure<ProviderOptions>(builder.Configuration.GetSection("Providers"));

// register our services
builder.Services.AddSingleton<PriceProviders>();

// register DB helper if needed (not DbContext)
builder.Services.AddSingleton<Db>();

var app = builder.Build();

// ensure DB structure
Db.EnsureCreated();

app.Run();