using HomeBot.Display;
using HomeBot.Discord;
using HomeBot.Integrations;
using HomeBot.Integrations.Jellyfin;
using HomeBot.Integrations.Prometheus;
using HomeBot.Integrations.QBittorrent;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddOpenApi();

var isOpenApiGeneration = Environment.CommandLine.Contains("getdocument", StringComparison.OrdinalIgnoreCase) ||
                          (System.Reflection.Assembly.GetEntryAssembly()?.FullName ?? "").Contains("getdocument", StringComparison.OrdinalIgnoreCase) ||
                          Environment.StackTrace.Contains("GetDocument", StringComparison.OrdinalIgnoreCase);

if (!isOpenApiGeneration)
{
    //
    // Discord
    //
    builder.Services
        .AddDiscordGateway()
        .AddApplicationCommands();
}

//
// Integrations
//
builder.Services.AddIntegration<JellyfinIntegration, JellyfinOptions>(
    builder.Configuration);

builder.Services.AddIntegration<PrometheusIntegration, PrometheusOptions>(
    builder.Configuration);

builder.Services.AddIntegration<QBittorrentIntegration, QBittorrentOptions>(
    builder.Configuration);

builder.Services.AddSingleton<IntegrationManager>();

builder.Services.AddSingleton<IntegrationMetricManager>();

if (!isOpenApiGeneration)
{
    builder.Services.AddHostedService<IntegrationMetricRefreshService>();
    builder.Services.AddHostedService<DiscordMetricChannelService>();
}

builder.Services.Configure<DiscordOptions>(
    builder.Configuration.GetSection("Discord"));

//
// Build
//
var app = builder.Build();

app.MapOpenApi();

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());

app.MapGet("/api/widgets/cards", async (IEnumerable<IWidget> widgets, CancellationToken ct) =>
{
    var cards = new List<WidgetSnapshotDto>();
    foreach (var widget in widgets)
    {
        var card = await widget.RenderAsync(ct);
        cards.Add(new WidgetSnapshotDto(
            widget.Id,
            widget.Name,
            widget.Description,
            card
        ));
    }
    return TypedResults.Ok(cards);
})
.WithName("GetWidgetCards")
.Produces<List<WidgetSnapshotDto>>(StatusCodes.Status200OK);

app.MapPost("/api/widgets/actions", async (ActionRequestDto request, IEnumerable<IWidget> widgets, CancellationToken ct) =>
{
    // For now, return a generic success acknowledgment
    return TypedResults.Ok(new ActionResponseDto(true, $"Action '{request.Label ?? request.ActionId}' executed.", null));
})
.WithName("ExecuteWidgetAction")
.Produces<ActionResponseDto>(StatusCodes.Status200OK);

//
// Register Discord modules only when not generating OpenAPI
//
if (!isOpenApiGeneration)
{
    app.AddModules(typeof(Program).Assembly);
}

//
// Run
//
await app.RunAsync();