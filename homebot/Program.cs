using Microsoft.Extensions.Hosting;

using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddDiscordGateway()
    .AddApplicationCommands();

var host = builder.Build();

// foreach (var c in builder.Configuration.AsEnumerable())
// {
//     Console.WriteLine($"CONFIG: Key: {c.Key} Value: {c.Value}");
// }

host.AddModules(typeof(Program).Assembly);

await host.RunAsync();
