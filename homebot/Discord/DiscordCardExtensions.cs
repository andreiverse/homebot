using HomeBot.Display;
using NetCord;
using NetCord.Rest;
using ScottPlot;

namespace HomeBot.Discord;

public static class DiscordCardExtensions
{
    public static EmbedProperties ToDiscordEmbed(this Card card)
    {
        var embed = new EmbedProperties();

        if (!string.IsNullOrWhiteSpace(card.Heading))
            embed.WithTitle(card.Heading);

        if (!string.IsNullOrWhiteSpace(card.Summary))
            embed.WithDescription(card.Summary);

        if (!string.IsNullOrWhiteSpace(card.Accent) &&
            !Uri.TryCreate(card.Accent, UriKind.Absolute, out _))
        {
            var hex = card.Accent.Trim().TrimStart('#');

            if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var rgb))
                embed.WithColor(new NetCord.Color(rgb));
        }

        if (card.Link is not null)
            embed.WithUrl(card.Link.ToString());

        if (card.CreatedAt is not null)
            embed.WithTimestamp(card.CreatedAt.Value);

        if (!string.IsNullOrWhiteSpace(card.Metadata.Author))
        {
            embed.WithAuthor(new EmbedAuthorProperties()
                .WithName(card.Metadata.Author));
        }

        if (!string.IsNullOrWhiteSpace(card.Metadata.Footer))
        {
            embed.WithFooter(new EmbedFooterProperties()
                .WithText(card.Metadata.Footer));
        }

        foreach (var block in card.Content)
        {
            switch (block)
            {
                case TextBlock text:
                {
                    if (!string.IsNullOrWhiteSpace(text.Heading))
                    {
                        embed.AddFields(new EmbedFieldProperties()
                            .WithName(text.Heading)
                            .WithValue(string.IsNullOrWhiteSpace(text.Text) ? "\u200B" : text.Text));
                    }
                    else if (!string.IsNullOrWhiteSpace(text.Text))
                    {
                        var description = embed.Description ?? string.Empty;

                        if (!string.IsNullOrEmpty(description))
                            description += "\n\n";

                        description += text.Text;
                        embed.WithDescription(description);
                    }

                    break;
                }

                case KeyValueBlock kv:
                {
                    foreach (var item in kv.Items)
                    {
                        embed.AddFields(new EmbedFieldProperties()
                            .WithName(item.Key)
                            .WithValue(item.Value)
                            .WithInline(true));
                    }

                    break;
                }

                case MediaBlock media:
                {
                    embed.WithImage(new EmbedImageProperties(media.Source.ToString()));
                    break;
                }

                case ListBlock list:
                {
                    var value = string.Join("\n", list.Items.Select(x => $"• {x}"));

                    embed.AddFields(new EmbedFieldProperties()
                        .WithName("List")
                        .WithValue(value));

                    break;
                }

                case Display.CodeBlock code:
                {
                    var value =
                        $"```{code.Language ?? ""}\n{code.Code}\n```";

                    embed.AddFields(new EmbedFieldProperties()
                        .WithName("Code")
                        .WithValue(value));

                    break;
                }

                case DividerBlock:
                    // Discord embeds have no divider.
                    break;

                case GraphBlock:
                    // The rendered PNG will be attached as graph.png;
                    // reference it here so Discord links the embed image.
                    embed.WithImage(new EmbedImageProperties("attachment://graph.png"));
                    break;
            }
        }

        return embed;
    }

    public static InteractionMessageProperties ToInteractionMessage(this Card card)
        => new() { Embeds = [card.ToDiscordEmbed()] };

    /// <summary>
    /// Converts a <see cref="Card"/> to an <see cref="InteractionMessageProperties"/>.
    /// If the card contains a <see cref="GraphBlock"/>, it is rendered to a PNG via ScottPlot
    /// and attached to the message automatically.
    /// </summary>
    public static async Task<InteractionMessageProperties> ToInteractionMessageAsync(
        this Card card,
        CancellationToken ct = default)
    {
        var graphBlock = card.Content.OfType<GraphBlock>().FirstOrDefault();

        if (graphBlock is null)
            return card.ToInteractionMessage();

        var stream = await RenderGraphAsync(graphBlock, ct);
        var embed = card.ToDiscordEmbed(); // already wired to attachment://graph.png

        return new()
        {
            Embeds = [embed],
            Attachments = [new AttachmentProperties("graph.png", stream)]
        };
    }

    private static async Task<MemoryStream> RenderGraphAsync(GraphBlock graph, CancellationToken ct)
    {
        var plot = new Plot();

        foreach (var series in graph.Series)
        {
            var scatter = plot.Add.Scatter(series.Xs, series.Ys);
            scatter.LineWidth = 5;

            if (series.Name is not null)
                scatter.LegendText = series.Name;
        }

        plot.Title(graph.Title);
        plot.XLabel(graph.XLabel);
        plot.YLabel(graph.YLabel);

        if (graph.IsDateTimeAxis)
            plot.Axes.DateTimeTicksBottom();

        plot.Axes.AutoScale();

        var file = Path.GetTempFileName() + ".png";
        plot.SavePng(file, 800, 400);

        var bytes = await File.ReadAllBytesAsync(file, ct);
        File.Delete(file);

        return new MemoryStream(bytes);
    }
}