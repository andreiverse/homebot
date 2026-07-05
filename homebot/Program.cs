using HomeBot.Integrations;
using HomeBot.Integrations.Jellyfin;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddDiscordGateway()
    .AddApplicationCommands();

// Jellyfin Integration
builder.Services.AddIntegration<JellyfinIntegration, JellyfinOptions>(builder.Configuration);

var host = builder.Build();

host.AddModules(typeof(Program).Assembly);
await host.RunAsync();
