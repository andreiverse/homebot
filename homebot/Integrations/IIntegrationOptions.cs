namespace HomeBot.Integrations;

public interface IIntegrationOptions
{
    bool Enabled { get; init; }
    public static abstract string Section { get; }
}