namespace HomeBot.Integrations;

public interface IIntegrationMetadata
{
    string Name { get; }
    string Description { get; }
}

public record IntegrationMetadata(string Name, string Description) : IIntegrationMetadata;