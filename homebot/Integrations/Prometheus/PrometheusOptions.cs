namespace HomeBot.Integrations.Prometheus;

public class PrometheusOptions : IIntegrationOptions
{
    public static string Section => "Prometheus";
    public bool Enabled { get; init; } = false;

    public required string Endpoint { get; init; }
    
}