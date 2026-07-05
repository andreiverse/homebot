using NetCord;

namespace HomeBot.Integrations;

public class IntegrationManager
{
    IEnumerable<IIntegration> integrations;

    public IntegrationManager(
        IEnumerable<IIntegration> integrations
    )
    {
        this.integrations = integrations;
    }

    public async Task<Dictionary<IIntegrationMetadata, IntegrationHealthStatus>> PerformHealthChecks()
    {
        var healthz = new Dictionary<IIntegrationMetadata, IntegrationHealthStatus>();

        foreach (var integration in integrations)
        {
            healthz.Add(integration.Metadata, await integration.PerformHealthCheck());
        }

        return healthz;
    }
}