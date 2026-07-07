using HomeBot.Integrations.Prometheus;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

public class PrometheusQueryAutocomplete(
    IOptions<PrometheusOptions> options)
    : IAutocompleteProvider<AutocompleteInteractionContext>
{
    private readonly PrometheusOptions _options = options.Value;

    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        var choices = _options.Queries
        .Take(25)
        .Select(q =>
        {
            var l = new ApplicationCommandOptionChoiceProperties(q.Name, "{PREDEFINED}:" + _options.Queries.IndexOf(q));
            return l;
        });

        return ValueTask.FromResult<IEnumerable<ApplicationCommandOptionChoiceProperties>?>(choices);
    }
}