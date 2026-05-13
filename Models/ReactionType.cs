using System.Text.Json.Serialization;

namespace Telebot.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ReactionTypeEmoji), "emoji")]
[JsonDerivedType(typeof(ReactionTypeCustomEmoji), "custom_emoji")]
[JsonDerivedType(typeof(ReactionTypePaid), "paid")]
public abstract record ReactionType;

public record ReactionTypeEmoji(
    [property: JsonPropertyName("emoji")]
    string Emoji
) : ReactionType;

public record ReactionTypeCustomEmoji(
    [property: JsonPropertyName("custom_emoji_id")]
    string CustomEmojiId
) : ReactionType;

public record ReactionTypePaid() : ReactionType;