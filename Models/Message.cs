using System.Text.Json.Serialization;

namespace Telebot.Models;

public record Message(
    [property: JsonPropertyName("message_id")]
    long MessageId,

    [property: JsonPropertyName("message_thread_id")]
    long? MessageThreadId,

    [property: JsonPropertyName("from")]
    User? From,

    [property: JsonPropertyName("sender_chat")]
    Chat? SenderChat,

    [property: JsonPropertyName("sender_boost_count")]
    int? SenderBoostCount,

    [property: JsonPropertyName("sender_business_bot")]
    User? SenderBusinessBot,

    [property: JsonPropertyName("date")]
    long Date,

    [property: JsonPropertyName("business_connection_id")]
    string? BusinessConnectionId,

    [property: JsonPropertyName("chat")]
    Chat Chat,

    [property: JsonPropertyName("is_topic_message")]
    bool? IsTopicMessage,

    [property: JsonPropertyName("is_automatic_forward")]
    bool? IsAutomaticForward,

    [property: JsonPropertyName("reply_to_message")]
    Message? ReplyToMessage,

    [property: JsonPropertyName("quote")]
    TextQuote? Quote,

    [property: JsonPropertyName("pinned_message")]
    Message? PinnedMessage,

    [property: JsonPropertyName("text")]
    string? Text
);