namespace HomeBot.Integrations;

public interface IMetricProvider
{
    IEnumerable<IntegrationMetricSnapshot> Snapshots { get; }
    Task RefreshAsync(CancellationToken cancellationToken = default);
    IEnumerable<IIntegrationMetric> Metrics { get; }
}