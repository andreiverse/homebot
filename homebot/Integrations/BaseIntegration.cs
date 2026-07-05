namespace HomeBot.Integrations;

public abstract class BaseIntegration : IIntegration
{
    public IIntegrationMetadata Metadata { get; }

    protected BaseIntegration(IIntegrationMetadata metadata)
    {
        Metadata = metadata;
    }

}