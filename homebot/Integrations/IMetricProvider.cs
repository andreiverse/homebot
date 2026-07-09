namespace HomeBot.Integrations;

public interface IMetricProvider
{
    IEnumerable<IIntegrationMetric> Metrics { get; }
}