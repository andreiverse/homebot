using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace HomeBot.Commands;

public class ExampleModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("health", "Check both health")]
    public static string Health() => "healthy";
}
