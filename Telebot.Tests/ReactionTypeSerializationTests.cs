using System.Text.Json;
using Telebot.Models;
using Xunit;

namespace Telebot.Tests;

public class ReactionTypeSerializationTests
{
    [Fact]
    public void Deserialize_EmojiReaction_Works()
    {
        var json = """
                   {
                       "type": "emoji",
                       "emoji": "👍"
                   }
                   """;

        var result = JsonSerializer.Deserialize<ReactionType>(json);

        Assert.NotNull(result);
        var emoji = Assert.IsType<ReactionTypeEmoji>(result);
        Assert.Equal("👍", emoji.Emoji);
    }

    [Fact]
    public void Deserialize_CustomEmojiReaction_Works()
    {
        var json = """
                   {
                       "type": "custom_emoji",
                       "custom_emoji_id": "1234567890"
                   }
                   """;

        var result = JsonSerializer.Deserialize<ReactionType>(json);

        Assert.NotNull(result);
        var custom = Assert.IsType<ReactionTypeCustomEmoji>(result);
        Assert.Equal("1234567890", custom.CustomEmojiId);
    }

    [Fact]
    public void Deserialize_PaidReaction_Works()
    {
        var json = """
                   {
                       "type": "paid"
                   }
                   """;

        var result = JsonSerializer.Deserialize<ReactionType>(json);

        Assert.NotNull(result);
        Assert.IsType<ReactionTypePaid>(result);
    }
}