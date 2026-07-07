using HomeBot.Display;
using NetCord;
using NetCord.Rest;

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

        if (Uri.TryCreate(card.Accent, UriKind.Absolute, out _))
        {
            // Accent isn't a URL, ignore.
        }
        else if (!string.IsNullOrWhiteSpace(card.Accent))
        {
            var hex = card.Accent.Trim().TrimStart('#');

            if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var rgb))
                embed.WithColor(new Color(rgb));
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
            }
        }

        return embed;
    }
}