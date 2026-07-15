using HomeBot.Display;
using HomeBot.Integrations.Prometheus;
using HomeBot.Integrations.Prometheus.Widgets;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace HomeBot.Discord.Modules;

[SlashCommand("prometheus", "Prometheus monitoring commands")]
public class PrometheusModule(
    PrometheusInfoWidget infoWidget,
    PrometheusTargetsWidget targetsWidget,
    PrometheusAlertsWidget alertsWidget,
    PrometheusQueryWidget queryWidget,
    PrometheusGraphWidget graphWidget)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("info", "Show Prometheus runtime information")]
    public async Task<InteractionMessageProperties> Info()
        => (await infoWidget.RenderAsync()).ToInteractionMessage();

    [SubSlashCommand("targets", "Show scrape target status")]
    public async Task<InteractionMessageProperties> Targets()
        => (await targetsWidget.RenderAsync()).ToInteractionMessage();

    [SubSlashCommand("alerts", "Show active alerts")]
    public async Task<InteractionMessageProperties> Alerts()
        => (await alertsWidget.RenderAsync()).ToInteractionMessage();

    [SubSlashCommand("query", "Execute a PromQL query")]
    public async Task<InteractionMessageProperties> Query(
        [SlashCommandParameter(Description = "PromQL expression")]
        string query)
        => (await queryWidget.RenderAsync(query)).ToInteractionMessage();

    [SubSlashCommand("graph", "Render a Prometheus graph")]
    public async Task<InteractionMessageProperties> Graph(
        [SlashCommandParameter(Description = "PromQL query", AutocompleteProviderType = typeof(PrometheusQueryAutocomplete))]
        string query,

        [SlashCommandParameter(Description = "Hours back")]
        int hours = 1)
        => await (await graphWidget.RenderAsync(query, hours)).ToInteractionMessageAsync();
}