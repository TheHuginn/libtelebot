using System.Text.Json.Serialization;

namespace Telebot;

/// <summary>
/// Параметры ответа на сообщение для метода <c>sendMessage</c> и других методов
/// отправки (см. <see href="https://core.telegram.org/bots/api#replyparameters"/>).
/// </summary>
/// <remarks>
/// Заменяет устаревшую пару <c>reply_to_message_id</c> + <c>allow_sending_without_reply</c>
/// (введено в Bot API 7.0, декабрь 2023). Позволяет не только сослаться на сообщение,
/// но и:
/// <list type="bullet">
///   <item><description>отвечать на сообщение из другого чата — через <see cref="ChatId"/>;</description></item>
///   <item><description>подсветить в цитате конкретный фрагмент — через <see cref="Quote"/> и <see cref="QuotePosition"/>.</description></item>
/// </list>
/// В Telegram Bot API этот объект передаётся как JSON-строка в поле формы
/// <c>reply_parameters</c> — сериализацию выполняет вызывающий запрос
/// (см. <see cref="SendMessageRequestParams.GetRequestFields"/>).
/// </remarks>
/// <param name="MessageId">
/// Идентификатор сообщения, на которое отвечаем в текущем чате
/// (или в <see cref="ChatId"/>, если он указан). Обязателен, если не задан
/// <see cref="EphemeralMessageId"/>.
/// </param>
/// <param name="ChatId">
/// Идентификатор чата, из которого сообщение-цель, если оно не в текущем чате.
/// В API поле принимает как число, так и строку <c>@username</c>; здесь оставлен
/// только числовой вариант ради консистентности с остальным API библиотеки.
/// </param>
/// <param name="EphemeralMessageId">
/// Идентификатор входящего эфемерного сообщения-цели в текущем чате.
/// Альтернатива <see cref="MessageId"/>: одно из двух обязано быть задано.
/// </param>
/// <param name="AllowSendingWithoutReply">
/// Если <c>true</c>, сообщение всё равно будет отправлено, даже если цель ответа
/// не найдена (удалена и т.п.).
/// </param>
/// <param name="Quote">
/// Цитируемый фрагмент исходного сообщения, 0–1024 символа после разбора форматирования.
/// </param>
/// <param name="QuoteParseMode">
/// Режим разбора разметки в <see cref="Quote"/>.
/// </param>
/// <param name="QuotePosition">
/// Позиция цитаты в исходном сообщении в UTF-16 code units.
/// </param>
/// <param name="ChecklistTaskId">
/// Идентификатор конкретной задачи чек-листа, на которую отвечаем.
/// </param>
/// <param name="PollOptionId">
/// Устойчивый идентификатор конкретного варианта опроса, на который отвечаем.
/// </param>
public sealed record ReplyParameters(
    [property: JsonPropertyName("message_id")] int? MessageId = null,
    [property: JsonPropertyName("chat_id")] long? ChatId = null,
    [property: JsonPropertyName("ephemeral_message_id")] int? EphemeralMessageId = null,
    [property: JsonPropertyName("allow_sending_without_reply")] bool? AllowSendingWithoutReply = null,
    [property: JsonPropertyName("quote")] string? Quote = null,
    [property: JsonPropertyName("quote_parse_mode")] string? QuoteParseMode = null,
    [property: JsonPropertyName("quote_position")] int? QuotePosition = null,
    [property: JsonPropertyName("checklist_task_id")] int? ChecklistTaskId = null,
    [property: JsonPropertyName("poll_option_id")] string? PollOptionId = null
);
