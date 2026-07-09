namespace HomeBot.Integrations;

public sealed record MetricSnapshot(
    IIntegrationMetric Metric,
    object? Value,
    DateTimeOffset Timestamp);