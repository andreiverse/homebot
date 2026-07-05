using NetCord.Services.ApplicationCommands;

namespace HomeBot.Discord.Modules;

public class HealthModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("health", "Check both health")]
    public static string Health() => "healthy";
}
