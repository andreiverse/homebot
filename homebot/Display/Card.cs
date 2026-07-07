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