namespace HomeBot.Integrations;

public sealed class IntegrationMetric : IIntegrationMetric
{
    public string Name { get; }
    public string Id { get; }

    public string? Description { get; }

    public IntegrationMetric(
        string name,
        string? description = null)
    {
        Name = name;
        Id = name.ToLower().Replace(" ", "_"); 
        Description = description;
    }

}