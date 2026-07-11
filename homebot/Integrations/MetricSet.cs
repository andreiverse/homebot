namespace HomeBot.Integrations;

/// <summary>
/// Registers a fixed set of metrics against one cached data object, deriving both
/// <see cref="IMetricProvider.Metrics"/> and <see cref="IMetricProvider.Snapshots"/> from a
/// single registration so the two can't drift out of sync.
/// </summary>
public sealed class MetricSet<TData>
{
    private sealed record Entry(IIntegrationMetric Metric, Func<TData, object?> Selector);

    private readonly List<Entry> _entries = [];

    public MetricSet<TData> Add(string name, string? description, Func<TData, object?> selector)
    {
        _entries.Add(new Entry(new IntegrationMetric(name, description), selector));
        return this;
    }

    public IReadOnlyCollection<IIntegrationMetric> Metrics =>
        _entries.Select(e => e.Metric).ToArray();

    public IReadOnlyCollection<IntegrationMetricSnapshot> Snapshot(TData? data, DateTimeOffset timestamp) =>
        _entries
            .Select(e => new IntegrationMetricSnapshot(
                e.Metric,
                data is null ? null : e.Selector(data),
                timestamp))
            .ToArray();
}
