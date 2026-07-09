using System.Collections.Concurrent;

namespace HomeBot.Integrations;

public sealed class IntegrationMetricManager
{
    private readonly Dictionary<string, IIntegrationMetric> _metrics = [];
    private readonly ConcurrentDictionary<string, IntegrationMetricSnapshot> _values = [];

    public IntegrationMetricManager(IEnumerable<IMetricProvider> providers)
    {
        foreach (var provider in providers)
        {
            foreach (var metric in provider.Metrics)
            {
                _metrics.Add(metric.Id, metric);
            }
        }
    }

    public IReadOnlyCollection<IIntegrationMetric> Metrics =>
        _metrics.Values;

    public async Task RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        var tasks = _metrics.Values.Select(async metric =>
        {
            var value = await metric.GetValueAsync(cancellationToken);

            _values[metric.Id] = new IntegrationMetricSnapshot(
                metric,
                value,
                DateTimeOffset.UtcNow);
        });

        await Task.WhenAll(tasks);
    }

    public IReadOnlyCollection<IntegrationMetricSnapshot> GetSnapshot()
        => _values.Values.ToArray();

    public IntegrationMetricSnapshot? GetSnapshot(string id)
    {
        _values.TryGetValue(id, out var snapshot);
        return snapshot;
    }

    public async Task<object?> GetValueAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!_metrics.TryGetValue(id, out var metric))
            throw new KeyNotFoundException(id);

        return await metric.GetValueAsync(cancellationToken);
    }
}