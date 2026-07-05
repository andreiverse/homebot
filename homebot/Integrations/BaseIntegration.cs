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

    public virtual Task InitializeAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public virtual Task ShutdownAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}