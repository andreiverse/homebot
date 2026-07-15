using HomeBot.Display;

namespace HomeBot.Integrations;

/// <summary>
/// A self-contained, renderable UI unit. Widgets receive their dependencies via
/// constructor injection and expose optional properties for configuration.
/// Call <see cref="RenderAsync"/> after setting any properties.
/// </summary>
public interface IWidget
{
    string Id { get; }
    string Name { get; }
    string? Description { get; }

    Task<Card> RenderAsync(CancellationToken ct = default);
}
