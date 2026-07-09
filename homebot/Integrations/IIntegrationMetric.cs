namespace HomeBot.Integrations;

public interface IIntegrationMetric
{
    string Id { get; }
    
    string Name { get; }

    ValueTask<object?> GetValueAsync(CancellationToken cancellationToken);
}