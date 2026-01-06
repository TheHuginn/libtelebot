using System.Text.Json.Serialization;

namespace Telebot.Models;

public record Update(
    [property: JsonPropertyName("update_id")]
    long UpdateId,

    [property: JsonPropertyName("message")]
    Message? Message = null,

    [property: JsonPropertyName("edited_message")]
    Message? EditedMessage = null,

    [property: JsonPropertyName("business_connection")]
    BusinessConnection? BusinessConnection = null,

    [property: JsonPropertyName("business_message")]
    Message? BusinessMessage = null,

    [property: JsonPropertyName("edited_business_message")]
    Message? EditedBusinessMessage = null,

    [property: JsonPropertyName("deleted_business_messages")]
    BusinessMessagesDeleted? DeletedBusinessMessages = null,

    [property: JsonPropertyName("my_chat_member")]
    ChatMemberUpdated? MyChatMember = null,

    [property: JsonPropertyName("chat_member")]
    ChatMemberUpdated? ChatMember = null
);