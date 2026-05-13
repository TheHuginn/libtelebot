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
    string? Text
);