using HomeBot.Display;

namespace HomeBot.Integrations;

public interface IIntegrationWidget<Integration> where Integration : IIntegration
{
    string Id { get; }
    string Name { get; }
    string? Description { get; }

    Task<Card> RenderAsync(
        Integration instance,
        CancellationToken cancellationToken = default);
}