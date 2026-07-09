namespace HomeBot.Integrations;

public sealed class IntegrationMetric : IIntegrationMetric
{
    private readonly Func<CancellationToken, ValueTask<object?>> _getter;

    public string Name { get; }
    public string Id { get; }

    public string? Description { get; }

    public IntegrationMetric(
        string name,
        Func<CancellationToken, ValueTask<object?>> getter,
        string? description = null)
    {
        Name = name;
        Id = name.ToLower().Replace(" ", "_");
        Description = description;
        _getter = getter;
    }

    public ValueTask<object?> GetValueAsync(
        CancellationToken cancellationToken = default)
        => _getter(cancellationToken);

}