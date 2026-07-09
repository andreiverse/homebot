using HomeBot.Integrations;
using Microsoft.Extensions.Options;
using NetCord.Rest;

namespace HomeBot.Discord;

public sealed class DiscordMetricChannelService : BackgroundService
{
    private readonly RestClient _client;
    private readonly IntegrationMetricManager _metrics;
    private readonly DiscordOptions _options;
    private readonly ILogger<DiscordMetricChannelService> _logger;

    private readonly Dictionary<ulong, string> _lastNames = [];

    public DiscordMetricChannelService(
        RestClient client,
        IntegrationMetricManager metrics,
        IOptions<DiscordOptions> options,
        ILogger<DiscordMetricChannelService> logger)
    {
        _client = client;
        _metrics = metrics;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await UpdateChannelsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update metric channels.");
            }
        }
    }

    private async Task UpdateChannelsAsync(
        CancellationToken cancellationToken)
    {
        var snapshot = _metrics.GetSnapshot()
            .ToDictionary(x => x.Metric.Id);

        foreach (var mapping in _options.MetricChannels)
        {
            if (!snapshot.TryGetValue(mapping.MetricId, out var metric))
                continue;

            var value = metric.Value?.ToString() ?? "N/A";

            var channelName = mapping.Format.Replace("{value}", value);

            if (_lastNames.TryGetValue(mapping.ChannelId, out var lastName) &&
                lastName == channelName)
            {
                continue;
            }

            var channel = await _client.ModifyGuildChannelAsync(mapping.ChannelId, (gs) =>
            {
                gs.Name = channelName;
            });

            _lastNames[mapping.ChannelId] = channelName;
        }
    }
}