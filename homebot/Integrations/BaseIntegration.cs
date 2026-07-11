using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HomeBot.Integrations;

public abstract class BaseIntegration<TSelf> : IIntegration
{
    public IIntegrationMetadata Metadata { get; }

    protected ILogger<TSelf> Logger { get; }

    protected BaseIntegration(
        ILogger<TSelf> logger,
        IIntegrationMetadata metadata)
    {
        Logger = logger;
        Metadata = metadata;
    }

    public virtual async Task<IntegrationHealthStatus> PerformHealthCheck()
    {
        return IntegrationHealthStatus.Unknown;
    }

    /// <summary>
    /// Runs <paramref name="probe"/> and maps it to a health status, treating any
    /// exception as Unhealthy so each integration doesn't repeat the same try/catch.
    /// </summary>
    protected static async Task<IntegrationHealthStatus> ProbeHealthAsync(
        Func<Task<bool>> probe)
    {
        try
        {
            return await probe()
                ? IntegrationHealthStatus.Healthy
                : IntegrationHealthStatus.Unhealthy;
        }
        catch
        {
            return IntegrationHealthStatus.Unhealthy;
        }
    }

    public virtual Task InitializeAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public virtual Task ShutdownAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}