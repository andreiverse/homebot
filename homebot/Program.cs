using HomeBot.Integrations.Jellyfin;
using Microsoft.Extensions.Options;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<JellyfinOptions>(
    builder.Configuration.GetSection(JellyfinOptions.Section));

builder.Services
    .AddDiscordGateway()
    .AddApplicationCommands();

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<JellyfinOptions>>().Value;
    
    return new JellyfinIntegration(
        options.Endpoint,
        options.ApiKey);
});

var host = builder.Build();

host.AddModules(typeof(Program).Assembly);
await host.RunAsync();
