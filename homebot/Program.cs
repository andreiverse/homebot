using HomeBot.Integrations;
using HomeBot.Integrations.Jellyfin;
using HomeBot.Integrations.Prometheus;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

var builder = Host.CreateApplicationBuilder(args);

//
// Discord
//
builder.Services
    .AddDiscordGateway()
    .AddApplicationCommands();

//
// Integrations
//
builder.Services.AddIntegration<JellyfinIntegration, JellyfinOptions>(
    builder.Configuration);
builder.Services.AddIntegration<PrometheusIntegration, PrometheusOptions>(
    builder.Configuration);
builder.Services.AddSingleton<IntegrationManager>();

builder.Services.AddSingleton<IntegrationMetricManager>();
builder.Services.AddHostedService<IntegrationMetricRefreshService>();

//
// Build
//
var host = builder.Build();

//
// Register Discord modules
//
host.AddModules(typeof(Program).Assembly);

//
// Run
//
await host.RunAsync();