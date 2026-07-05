namespace HomeBot.Integrations;

public interface IIntegration
{
    IIntegrationMetadata Metadata { get; }
    Task InitializeAsync(CancellationToken cancellationToken);
}