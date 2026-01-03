using System.Text.Json.Serialization;

namespace Telebot.Models;

public abstract record BusinessMessagesDeleted(
    [property: JsonPropertyName("business_connection_id")]
    string BusinessConnectionId,

    [property: JsonPropertyName("chat")]
    Chat Chat,

    [property: JsonPropertyName("message_ids")]
    IReadOnlyList<long> MessageIds
);

public record BusinessBotRights(
    [property: JsonPropertyName("can_reply")]
    bool? CanReply,

    [property: JsonPropertyName("can_read_messages")]
    bool? CanReadMessages,

    [property: JsonPropertyName("can_delete_sent_messages")]
    bool? CanDeleteSentMessages
);

public record BusinessConnection(
    [property: JsonPropertyName("id")]
    string Id,

    [property: JsonPropertyName("user")]
    User User,

    [property: JsonPropertyName("user_chat_id")]
    long UserChatId,

    [property: JsonPropertyName("date")]
    long Date,

    [property: JsonPropertyName("rights")]
    BusinessBotRights? Rights,

    [property: JsonPropertyName("is_enabled")]
    bool IsEnabled
);

public record BusinessLocation(
    [property: JsonPropertyName("address")]
    string Address,

    [property: JsonPropertyName("location")]
    Location? Location
);