namespace HomeBot.Integrations;

public sealed class IntegrationMetricManager
{
    private readonly IReadOnlyList<IMetricProvider> _providers;
    private readonly Dictionary<string, IIntegrationMetric> _metrics;

    public IntegrationMetricManager(IEnumerable<IMetricProvider> providers)
    {
        _providers = providers.ToList();

        _metrics = _providers
            .SelectMany(p => p.Metrics)
            .ToDictionary(m => m.Id);
    }

    public IReadOnlyCollection<IIntegrationMetric> Metrics =>
        _metrics.Values;

    public async Task RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            _providers.Select(p => p.RefreshAsync(cancellationToken)));
    }

    public IReadOnlyCollection<IntegrationMetricSnapshot> GetSnapshot()
    {
        return _providers
            .SelectMany(p => p.Snapshots)
            .ToArray();
    }

    public IntegrationMetricSnapshot? GetSnapshot(string id)
    {
        return _providers
            .SelectMany(p => p.Snapshots)
            .FirstOrDefault(s => s.Metric.Id == id);
    }

    public IIntegrationMetric? GetMetric(string id)
    {
        _metrics.TryGetValue(id, out var metric);
        return metric;
    }
}