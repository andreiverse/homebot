namespace HomeBot.Display;

public class Card
{
    public string? Heading { get; set; }
    public string? Summary { get; set; }

    /// <summary>Main accent if the platform supports it.</summary>
    public string? Accent { get; set; }

    public Uri? Link { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public List<ContentBlock> Content { get; set; } = [];
    public List<ActionItem> Actions { get; set; } = [];
    public Metadata Metadata { get; set; } = new();

    // Fluent API / Builder Methods

    public Card WithHeading(string? heading)
    {
        Heading = heading;
        return this;
    }

    public Card WithSummary(string? summary)
    {
        Summary = summary;
        return this;
    }

    public Card WithAccent(string? accent)
    {
        Accent = accent;
        return this;
    }

    public Card WithLink(Uri? link)
    {
        Link = link;
        return this;
    }

    public Card WithLink(string url)
    {
        Link = Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null;
        return this;
    }

    public Card WithCreatedAt(DateTimeOffset? createdAt)
    {
        CreatedAt = createdAt;
        return this;
    }

    public Card WithMetadata(Action<Metadata> configure)
    {
        configure(Metadata);
        return this;
    }

    public Card AddContent(ContentBlock block)
    {
        Content.Add(block);
        return this;
    }

    public Card AddTextBlock(string text, string? heading = null)
    {
        Content.Add(new TextBlock { Text = text, Heading = heading });
        return this;
    }

    public Card AddKeyValueBlock(Action<KeyValueBlockBuilder> configure)
    {
        var builder = new KeyValueBlockBuilder();
        configure(builder);
        Content.Add(builder.Build());
        return this;
    }

    public Card AddMediaBlock(Uri source, string? description = null)
    {
        Content.Add(new MediaBlock { Source = source, Description = description });
        return this;
    }

    public Card AddMediaBlock(string sourceUrl, string? description = null)
    {
        if (Uri.TryCreate(sourceUrl, UriKind.Absolute, out var uri))
        {
            Content.Add(new MediaBlock { Source = uri, Description = description });
        }
        return this;
    }

    public Card AddListBlock(IEnumerable<string> items)
    {
        Content.Add(new ListBlock { Items = items.ToList() });
        return this;
    }

    public Card AddListBlock(params string[] items)
    {
        Content.Add(new ListBlock { Items = items.ToList() });
        return this;
    }

    public Card AddCodeBlock(string code, string? language = null)
    {
        Content.Add(new CodeBlock { Code = code, Language = language });
        return this;
    }

    public Card AddDivider()
    {
        Content.Add(new DividerBlock());
        return this;
    }

    /// <summary>
    /// Adds a time-series graph block using <see cref="DateTimeOffset"/> x-values.
    /// Adapters (e.g. Discord) will render this as an image.
    /// </summary>
    public Card AddTimeSeries(
        string title,
        IEnumerable<(DateTimeOffset Time, double Value)> points,
        string yLabel = "Value",
        string? seriesName = null)
    {
        var pts = points.ToList();
        return AddGraph(
            title,
            xs: pts.Select(p => p.Time.UtcDateTime.ToOADate()).ToArray(),
            ys: pts.Select(p => p.Value).ToArray(),
            isDateTimeAxis: true,
            xLabel: "Time",
            yLabel: yLabel,
            seriesName: seriesName);
    }

    /// <summary>
    /// Adds a generic graph block with raw double x/y arrays.
    /// </summary>
    public Card AddGraph(
        string title,
        double[] xs,
        double[] ys,
        bool isDateTimeAxis = false,
        string xLabel = "X",
        string yLabel = "Y",
        string? seriesName = null)
    {
        Content.Add(new GraphBlock
        {
            Title = title,
            XLabel = xLabel,
            YLabel = yLabel,
            IsDateTimeAxis = isDateTimeAxis,
            Series = [new GraphSeries { Name = seriesName, Xs = xs, Ys = ys }]
        });
        return this;
    }

    public Card AddAction(string label, string? url = null, string? id = null)
    {
        Actions.Add(new ActionItem
        {
            Label = label,
            Url = url != null && Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null,
            Id = id
        });
        return this;
    }
}

public class KeyValueBlockBuilder
{
    private readonly List<KeyValueItem> _items = [];

    public KeyValueBlockBuilder Add(string key, string value)
    {
        _items.Add(new KeyValueItem { Key = key, Value = value });
        return this;
    }

    public KeyValueBlockBuilder Add(string key, object? value)
    {
        _items.Add(new KeyValueItem { Key = key, Value = value?.ToString() ?? "N/A" });
        return this;
    }

    public KeyValueBlock Build()
    {
        return new KeyValueBlock { Items = _items };
    }
}

public abstract class ContentBlock;

public class TextBlock : ContentBlock
{
    public string? Heading { get; set; }
    public string Text { get; set; } = "";
}

public class KeyValueBlock : ContentBlock
{
    public List<KeyValueItem> Items { get; set; } = [];
}

public class MediaBlock : ContentBlock
{
    public Uri Source { get; set; } = null!;
    public string? Description { get; set; }
}

public class ListBlock : ContentBlock
{
    public List<string> Items { get; set; } = [];
}

public class CodeBlock : ContentBlock
{
    public string Code { get; set; } = "";
    public string? Language { get; set; }
}

public class DividerBlock : ContentBlock;

public class GraphBlock : ContentBlock
{
    public string Title { get; set; } = "";
    public string XLabel { get; set; } = "X";
    public string YLabel { get; set; } = "Y";

    /// <summary>When true the X axis represents <see cref="DateTime.ToOADate"/> values.</summary>
    public bool IsDateTimeAxis { get; set; }

    public List<GraphSeries> Series { get; set; } = [];
}

public class GraphSeries
{
    public string? Name { get; set; }
    public double[] Xs { get; set; } = [];
    public double[] Ys { get; set; } = [];
}

public class KeyValueItem
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}

public class ActionItem
{
    public string Label { get; set; } = "";
    public Uri? Url { get; set; }
    public string? Id { get; set; }
}

public class Metadata
{
    public string? Author { get; set; }
    public string? Source { get; set; }
    public string? Footer { get; set; }
}