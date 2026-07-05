namespace HomeBot.Integrations;

public interface IIntegrationMetadata
{
    String Name { get; }
    String Description { get; }
}

public record IntegrationMetadata(string Name, string Description) : IIntegrationMetadata;