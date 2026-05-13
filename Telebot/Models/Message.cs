using System.Text.Json.Serialization;

namespace Telebot.Models;

public record Message(
    [property: JsonPropertyName("message_id")]
    int MessageId,

    [property: JsonPropertyName("message_thread_id")]
    int? MessageThreadId,

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
    string? Text,

    [property: JsonPropertyName("entities")]
    MessageEntity[]? Entities
);

/// <summary>
/// Представляет специальную сущность внутри текста сообщения —
/// упоминание, хэштег, ссылку, форматирование и т. п.
/// (см. <see href="https://core.telegram.org/bots/api#messageentity"/>).
/// </summary>
/// <remarks>
/// Сущность описывает диапазон символов исходного текста и его тип.
/// Важно: <see cref="Offset"/> и <see cref="Length"/> измеряются в
/// <b>UTF-16 code units</b>, а не в Unicode-кодовых точках. Это означает,
/// что эмодзи и символы вне BMP занимают по 2 единицы. В .NET строки уже
/// хранятся в UTF-16, поэтому <c>text.Substring(offset, length)</c> вернёт
/// корректную подстроку без дополнительных пересчётов.
/// <para/>
/// Большинство полей кроме <see cref="Type"/>, <see cref="Offset"/> и
/// <see cref="Length"/> относятся только к конкретному значению <see cref="Type"/>:
/// <list type="bullet">
///   <item><description><see cref="Url"/> — только для <c>text_link</c>.</description></item>
///   <item><description><see cref="User"/> — только для <c>text_mention</c>.</description></item>
///   <item><description><see cref="Language"/> — только для <c>pre</c>.</description></item>
///   <item><description><see cref="CustomEmojiId"/> — только для <c>custom_emoji</c>.</description></item>
/// </list>
/// </remarks>
/// <param name="Type">
/// Тип сущности. Возможные значения: <c>mention</c> (<c>@username</c>),
/// <c>hashtag</c>, <c>cashtag</c>, <c>bot_command</c>, <c>url</c>, <c>email</c>,
/// <c>phone_number</c>, <c>bold</c>, <c>italic</c>, <c>underline</c>,
/// <c>strikethrough</c>, <c>spoiler</c>, <c>blockquote</c>,
/// <c>expandable_blockquote</c>, <c>code</c>, <c>pre</c>, <c>text_link</c>,
/// <c>text_mention</c>, <c>custom_emoji</c>.
/// </param>
/// <param name="Offset">
/// Смещение в UTF-16 code units от начала текста до начала сущности.
/// </param>
/// <param name="Length">
/// Длина сущности в UTF-16 code units.
/// </param>
/// <param name="Url">
/// URL, открываемый при нажатии. Заполняется только для <c>text_link</c>.
/// </param>
/// <param name="User">
/// Упомянутый пользователь. Заполняется только для <c>text_mention</c>
/// (упоминание пользователя без <c>@username</c>).
/// </param>
/// <param name="Language">
/// Язык программирования для блока кода. Заполняется только для <c>pre</c>.
/// </param>
/// <param name="CustomEmojiId">
/// Уникальный идентификатор кастомного эмодзи-стикера.
/// Заполняется только для <c>custom_emoji</c>; получить сам стикер можно
/// через метод <c>getCustomEmojiStickers</c>.
/// </param>
public record MessageEntity(
    [property: JsonPropertyName("type")]
    string Type,

    [property: JsonPropertyName("offset")]
    int Offset,

    [property: JsonPropertyName("length")]
    int Length,

    [property: JsonPropertyName("url")]
    string? Url = null,

    [property: JsonPropertyName("user")]
    User? User = null,

    [property: JsonPropertyName("language")]
    string? Language = null,

    [property: JsonPropertyName("custom_emoji_id")]
    string? CustomEmojiId = null
);