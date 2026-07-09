namespace HomeBot.Integrations;

public sealed record IntegrationMetricSnapshot(
    IIntegrationMetric Metric,
    object? Value,
    DateTimeOffset Timestamp);