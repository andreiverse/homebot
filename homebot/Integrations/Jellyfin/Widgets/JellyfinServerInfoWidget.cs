using HomeBot.Display;

namespace HomeBot.Integrations.Jellyfin.Widgets;

public class JellyfinServerInfoWidget : IIntegrationWidget<JellyfinIntegration>
{
    public string Id => "serverinfo";
    public string Name => "Server Info";
    public string? Description => "Shows useful Jellyfin server info";

    public async Task<Card> RenderAsync(JellyfinIntegration instance, CancellationToken cancellationToken = default)
    {
        var sysInfo = await instance.GetSystemInfoAsync();

        return new Card
        {
            Heading = "📺 Jellyfin Server",
            Content =
            [
                new KeyValueBlock
                {
                    Items =
                    [
                        new()
                        {
                            Key = "Server",
                            Value = sysInfo.ServerName
                        },
                        new()
                        {
                            Key = "Version",
                            Value = sysInfo.Version
                        },
                        new()
                        {
                            Key = "Product",
                            Value = sysInfo.ProductName
                        },
                        new()
                        {
                            Key = "Update",
                            Value = sysInfo.HasUpdateAvailable
                                ? "🟡 Available"
                                : "🟢 Up to date"
                        }
                    ]
                }
            ]
        };

    }
}