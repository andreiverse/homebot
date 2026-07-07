namespace HomeBot.Integrations.Prometheus;

public class PrometheusQuery
{
    
    public string Name { get; set; } = "";
    public string PromQL { get; set; } = "";
}

public class PrometheusOptions : IIntegrationOptions
{
    public static string Section => "Prometheus";
    public bool Enabled { get; init; } = false;
    public bool OnlyAllowDefinedQueries { get; init; } = true; 

    public required string Endpoint { get; init; }
    public List<PrometheusQuery> Queries { get; init; } = [];
}