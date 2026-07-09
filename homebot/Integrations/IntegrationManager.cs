namespace HomeBot.Integrations;

public class IntegrationManager
{
    readonly IEnumerable<IIntegration> integrations;

    public IntegrationManager(
        IEnumerable<IIntegration> integrations
    )
    {
        this.integrations = integrations;
    }

    public async Task<Dictionary<IIntegrationMetadata, IntegrationHealthStatus>> PerformHealthChecks()
    {
        var tasks = integrations.Select(async integration => new
        {
            integration.Metadata,
            Status = await integration.PerformHealthCheck()
        });

        var results = await Task.WhenAll(tasks);

        return results.ToDictionary(x => x.Metadata, x => x.Status);
    }
}