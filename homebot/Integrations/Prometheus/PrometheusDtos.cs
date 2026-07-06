using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeBot.Integrations.Prometheus;

public sealed class PrometheusRuntimeInfoResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("data")]
    public PrometheusRuntimeInfo Data { get; set; } = new();
}

public sealed class PrometheusRuntimeInfo
{
    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("CWD")]
    public string CurrentWorkingDirectory { get; set; } = "";

    [JsonPropertyName("reloadConfigSuccess")]
    public bool ReloadConfigSuccess { get; set; }

    [JsonPropertyName("lastConfigTime")]
    public DateTime LastConfigTime { get; set; }

    [JsonPropertyName("corruptionCount")]
    public int CorruptionCount { get; set; }

    [JsonPropertyName("goroutineCount")]
    public int GoroutineCount { get; set; }

    [JsonPropertyName("GOMAXPROCS")]
    public int GoMaxProcs { get; set; }

    [JsonPropertyName("GOGC")]
    public string GoGc { get; set; } = "";

    [JsonPropertyName("GODEBUG")]
    public string GoDebug { get; set; } = "";

    [JsonPropertyName("storageRetention")]
    public string StorageRetention { get; set; } = "";

    [JsonPropertyName("timeSeriesCount")]
    public long TimeSeriesCount { get; set; }
}

public sealed class PrometheusQueryResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("data")]
    public PrometheusQueryData Data { get; set; } = new();
}

public sealed class PrometheusQueryData
{
    [JsonPropertyName("resultType")]
    public string ResultType { get; set; } = "";

    [JsonPropertyName("result")]
    public List<PrometheusVectorResult> Result { get; set; } = [];
}

public sealed class PrometheusVectorResult
{
    [JsonPropertyName("metric")]
    public Dictionary<string, string> Metric { get; set; } = [];

    [JsonPropertyName("value")]
    public List<object> Value { get; set; } = [];
}

public sealed class PrometheusRangeQueryResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("data")]
    public PrometheusRangeQueryData Data { get; set; } = new();
}

public sealed class PrometheusRangeQueryData
{
    [JsonPropertyName("resultType")]
    public string ResultType { get; set; } = "";

    [JsonPropertyName("result")]
    public List<PrometheusMatrixResult> Result { get; set; } = [];
}

public sealed class PrometheusMatrixResult
{
    [JsonPropertyName("metric")]
    public Dictionary<string, string> Metric { get; set; } = [];

    [JsonPropertyName("values")]
    public List<List<JsonElement>> Values { get; set; } = [];
}

public sealed class PrometheusTargetsResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("data")]
    public PrometheusTargetsData Data { get; set; } = new();
}

public sealed class PrometheusDroppedTarget
{
    [JsonPropertyName("discoveredLabels")]
    public Dictionary<string, string> DiscoveredLabels { get; set; } = [];
}

public sealed class PrometheusTargetsData
{
    [JsonPropertyName("activeTargets")]
    public List<PrometheusActiveTarget> ActiveTargets { get; set; } = [];

    [JsonPropertyName("droppedTargets")]
    public List<PrometheusDroppedTarget> DroppedTargets { get; set; } = [];
}

public sealed class PrometheusActiveTarget
{
    [JsonPropertyName("health")]
    public string Health { get; set; } = "";

    [JsonPropertyName("scrapeUrl")]
    public string ScrapeUrl { get; set; } = "";

    [JsonPropertyName("lastScrape")]
    public DateTimeOffset LastScrape { get; set; }

    [JsonPropertyName("lastError")]
    public string LastError { get; set; } = "";

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("discoveredLabels")]
    public Dictionary<string, string> DiscoveredLabels { get; set; } = [];
}

public sealed class PrometheusAlertsResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("data")]
    public PrometheusAlertsData Data { get; set; } = new();
}

public sealed class PrometheusAlertsData
{
    [JsonPropertyName("alerts")]
    public List<PrometheusAlert> Alerts { get; set; } = [];
}

public sealed class PrometheusAlert
{
    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("annotations")]
    public Dictionary<string, string> Annotations { get; set; } = [];

    [JsonPropertyName("state")]
    public string State { get; set; } = "";

    [JsonPropertyName("activeAt")]
    public DateTimeOffset ActiveAt { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; } = "";
}