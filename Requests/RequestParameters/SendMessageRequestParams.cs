using System.Text.Json;

namespace Telebot;

/// <summary>
/// Параметры вызова метода <c>sendMessage</c> — отправка текстового сообщения
/// в чат. <see cref="ChatId"/> и <see cref="Text"/> обязательны, остальные
/// поля управляют форматированием, уведомлениями и привязкой к ответам.
/// </summary>
/// <param name="ChatId">Идентификатор чата-получателя (пользователь, группа или канал).</param>
/// <param name="Text">Текст сообщения, до 4096 символов после разбора форматирования.</param>
/// <param name="MessageThreadId">
/// Идентификатор треда внутри супергруппы с форумом — позволяет отправить
/// сообщение в конкретную тему.
/// </param>
/// <param name="ParseMode">
/// Режим разбора разметки в <see cref="Text"/>: <c>Markdown</c>, <c>MarkdownV2</c>
/// или <c>HTML</c>. Если не задан — текст уходит как plain text.
/// </param>
/// <param name="LinkPreviewOptions">
/// Настройки превью ссылок в сообщении (см. <see cref="Telebot.LinkPreviewOptions"/>).
/// Пришли на смену устаревшему <c>disable_web_page_preview</c> в Bot API 7.0.
/// </param>
/// <param name="DisableNotification">
/// Если <c>true</c>, сообщение придёт без звука — полезно для фоновых уведомлений.
/// </param>
/// <param name="ProtectContent">
/// Если <c>true</c>, Telegram запретит пересылку и сохранение сообщения.
/// </param>
/// <param name="ReplyParameters">
/// Параметры ответа на другое сообщение (см. <see cref="Telebot.ReplyParameters"/>).
/// Пришли на смену устаревшим <c>reply_to_message_id</c> и
/// <c>allow_sending_without_reply</c> в Bot API 7.0.
/// </param>
/// <param name="ReplyMarkup">
/// Разметка под сообщением: инлайн-клавиатура, обычная клавиатура,
/// её удаление или force-reply (см. <see cref="IReplyMarkup"/>). Union без явного
/// тега — Telegram распознаёт вариант по маркерному полю в JSON,
/// поэтому сериализацию каждой реализации знает она сама через
/// <see cref="IReplyMarkup.ToJson"/>.
/// </param>
public sealed record SendMessageRequestParams(
    long ChatId,
    string Text,
    int? MessageThreadId = null,
    string? ParseMode = null,
    LinkPreviewOptions? LinkPreviewOptions = null,
    bool? DisableNotification = null,
    bool? ProtectContent = null,
    ReplyParameters? ReplyParameters = null,
    IReplyMarkup? ReplyMarkup = null
) : TelegramRequest("sendMessage")
{
    /// <summary>
    /// Обязательные поля (chat_id, text) выдаются всегда; остальные —
    /// только если явно заданы. Булевы значения сериализуются как литералы
    /// <c>"true"</c>/<c>"false"</c>, как этого требует Bot API.
    /// <see cref="ReplyParameters"/> и <see cref="ReplyMarkup"/> уходят как
    /// JSON-строки: Telegram ожидает именно такое представление составных
    /// объектов в form-запросе.
    /// </summary>
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("chat_id", ChatId.ToString());
        yield return new TelegramRequestField("text", Text);

        if(MessageThreadId is not null)
            yield return new TelegramRequestField("message_thread_id", MessageThreadId.Value.ToString());
        if (ParseMode is not null)
            yield return new TelegramRequestField("parse_mode", ParseMode);
        if (LinkPreviewOptions is not null)
            yield return new TelegramRequestField("link_preview_options",
                JsonSerializer.Serialize(LinkPreviewOptions, TelebotJson.Options));
        if (DisableNotification is not null)
            yield return new TelegramRequestField("disable_notification",
                DisableNotification.Value ? "true" : "false");
        if (ProtectContent is not null)
            yield return new TelegramRequestField("protect_content",
                ProtectContent.Value ? "true" : "false");
        if (ReplyParameters is not null)
            yield return new TelegramRequestField("reply_parameters",
                JsonSerializer.Serialize(ReplyParameters, TelebotJson.Options));
        if (ReplyMarkup is not null)
            yield return new TelegramRequestField("reply_markup", ReplyMarkup.ToJson());
    }
}
