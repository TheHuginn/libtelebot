using System.Text.Json.Serialization;

namespace Telebot.Models;

/// <summary>
/// Представляет сообщение в Telegram
/// (см. <see href="https://core.telegram.org/bots/api#message"/>).
/// </summary>
/// <remarks>
/// Сообщение — основная единица содержимого, которой обмениваются пользователи
/// и боты. Объект имеет много необязательных полей: одновременно заполнены только
/// те, что соответствуют конкретному типу сообщения (текст, медиа, служебное событие
/// и т. п.). Чтобы определить тип сообщения, проверяйте, какое из полей не равно
/// <c>null</c>.
/// <para/>
/// Внимание: данная модель содержит лишь часть полей, описанных в официальной
/// документации. По мере необходимости остальные поля могут быть добавлены.
/// </remarks>
/// <param name="MessageId">
/// Уникальный идентификатор сообщения внутри чата.
/// В супергруппах и каналах идентификаторы возрастают монотонно,
/// но между разными чатами не сравнимы.
/// </param>
/// <param name="MessageThreadId">
/// Идентификатор треда (топика) форума, к которому относится сообщение.
/// Присутствует только для сообщений в супергруппах с включёнными темами.
/// </param>
/// <param name="From">
/// Отправитель сообщения. Может быть <c>null</c> для сообщений в каналах
/// и автоматических пересылок из связанных каналов.
/// </param>
/// <param name="SenderChat">
/// Чат, от имени которого отправлено сообщение
/// (анонимный администратор группы, канал, привязанный к супергруппе, и т. п.).
/// </param>
/// <param name="SenderBoostCount">
/// Количество бустов, которые отправитель добавил чату.
/// Заполняется, если отправитель буст-нул чат.
/// </param>
/// <param name="SenderBusinessBot">
/// Бот, через которого было отправлено сообщение от имени бизнес-аккаунта.
/// </param>
/// <param name="Date">
/// Дата отправки сообщения в Unix-времени (секунды с эпохи).
/// </param>
/// <param name="BusinessConnectionId">
/// Идентификатор бизнес-подключения, в рамках которого отправлено сообщение.
/// </param>
/// <param name="Chat">
/// Чат, в котором находится сообщение. Обязательное поле.
/// </param>
/// <param name="IsTopicMessage">
/// <c>true</c>, если сообщение принадлежит топику форума.
/// </param>
/// <param name="IsAutomaticForward">
/// <c>true</c>, если сообщение является автоматической пересылкой
/// из связанного канала в дискуссионную группу.
/// </param>
/// <param name="ReplyToMessage">
/// Сообщение, на которое отвечает текущее. Telegram не включает рекурсивно
/// поле <see cref="ReplyToMessage"/> у этого вложенного объекта,
/// чтобы избежать неограниченной вложенности.
/// </param>
/// <param name="Quote">
/// Цитируемый фрагмент исходного сообщения. Может присутствовать,
/// даже если <see cref="ReplyToMessage"/> также заполнено.
/// </param>
/// <param name="PinnedMessage">
/// Сообщение, закреплённое в чате. Заполняется для служебных сообщений
/// о закреплении (то есть когда текущее сообщение — это уведомление о пине).
/// </param>
/// <param name="Text">
/// Текст сообщения (UTF-8). Для текстовых сообщений — до 4096 символов.
/// </param>
/// <param name="Entities">
/// Специальные сущности в тексте сообщения — упоминания, хэштеги, URL,
/// форматирование и т. п. Заполняется только для текстовых сообщений
/// и только если такие сущности присутствуют. Соответствует полю
/// <c>entities</c> в Telegram Bot API; элементы массива описаны
/// типом <see cref="MessageEntity"/>.
/// </param>
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