namespace HomeBot.Discord;

public sealed class DiscordOptions
{
    public List<MetricChannelOptions> MetricChannels { get; init; } = [];
}

public sealed class MetricChannelOptions
{
    public ulong ChannelId { get; init; }

    public string MetricId { get; init; } = "";

    public string Format { get; init; } = "{value}";
}