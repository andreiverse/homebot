using System.Text.Json;
using HomeBot.Display;
using HomeBot.Integrations;
using ModelContextProtocol.Server;

namespace HomeBot.Mcp;

/// <summary>
/// Registers every <see cref="IWidget"/> as a dynamic MCP tool at startup.
/// Each tool is named after the widget's <see cref="IWidget.Id"/> and calls
/// <see cref="IWidget.RenderAsync"/> when invoked, returning the rendered
/// <see cref="Card"/> as JSON.
/// </summary>
public static class WidgetMcpToolSource
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new StreamBase64Converter() }
    };

    /// <summary>
    /// Discovers all registered <see cref="IWidget"/> implementations and creates
    /// an <see cref="McpServerTool"/> for each one. Widgets are transient, so each
    /// tool invocation creates a fresh scope to resolve the widget's concrete type.
    /// </summary>
    public static IReadOnlyList<McpServerTool> CreateToolsFromWidgets(
        IServiceProvider services)
    {
        // Resolve all widget instances once just to read their metadata.
        // The actual tool invocations will create fresh scopes.
        using var scope = services.CreateScope();
        var widgets = scope.ServiceProvider.GetServices<IWidget>().ToList();

        var tools = new List<McpServerTool>();

        foreach (var widget in widgets)
        {
            var concreteType = widget.GetType();
            var id = widget.Id;
            var name = widget.Name;
            var description = widget.Description ?? $"Renders the {name} widget";
            description += "\n\nIMPORTANT: If the returned JSON contains an 'imageStream' field with a base64 Data URI, you MUST display it to the user in your response using standard Markdown image syntax: ![Widget Image](data:image/png;base64,...)";

            if (widget.OptionsType != null)
            {
                var method = typeof(WidgetMcpToolSource).GetMethod(nameof(CreateToolWithOptions), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
                var genericMethod = method.MakeGenericMethod(widget.OptionsType);
                var tool = (McpServerTool)genericMethod.Invoke(null, [concreteType, id, name, description])!;
                tools.Add(tool);
            }
            else
            {
                tools.Add(McpServerTool.Create(
                    async (IServiceProvider sp, CancellationToken ct) =>
                    {
                        var w = (IWidget)sp.GetRequiredService(concreteType);
                        return await RenderWidgetAsync(w, null, ct);
                    },
                    new McpServerToolCreateOptions
                    {
                        Name = id,
                        Title = name,
                        Description = description,
                    }));
            }
        }

        return tools;
    }

    private static McpServerTool CreateToolWithOptions<TOptions>(
        Type concreteType, string id, string name, string description)
    {
        return McpServerTool.Create(
            async (TOptions options, IServiceProvider sp, CancellationToken ct) =>
            {
                var w = (IWidget)sp.GetRequiredService(concreteType);
                return await RenderWidgetAsync(w, options, ct);
            },
            new McpServerToolCreateOptions
            {
                Name = id,
                Title = name,
                Description = description,
            });
    }

    private static async Task<string> RenderWidgetAsync(
        IWidget widget, object? options, CancellationToken ct)
    {
        var card = await widget.RenderAsync(options, ct);

        var snapshot = new WidgetSnapshotDto(
            widget.Id,
            widget.Name,
            widget.Description,
            card);

        return JsonSerializer.Serialize(snapshot, JsonOptions);
    }
}
