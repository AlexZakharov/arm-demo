using CryptoPriceAnalyzer.Data;
using CryptoPriceAnalyzer.Options;
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

builder.Configuration
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// register options
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<ProviderOptions>(builder.Configuration.GetSection("Providers"));

// register services
builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<PriceProviders>();

var app = builder.Build();

// ensure DB structure
var db = app.Services.GetRequiredService<Database>();
db.EnsureCreated();

app.Run();