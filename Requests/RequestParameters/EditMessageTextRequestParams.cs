using System.Text.Json;

namespace Telebot;

/// <summary>
/// Параметры вызова метода <c>editMessageText</c> — редактирование текста
/// ранее отправленного ботом сообщения. <see cref="ChatId"/>,
/// <see cref="MessageId"/> и <see cref="Text"/> обязательны.
/// </summary>
/// <remarks>
/// Метод API имеет второй вариант вызова — по <c>inline_message_id</c>, когда
/// правится сообщение, отправленное через инлайн-режим. В библиотеке инлайн-режим
/// пока не поддерживается (нет соответствующих типов), поэтому здесь оставлен
/// только вариант с <c>chat_id + message_id</c>, возвращающий <see cref="Telebot.Models.Message"/>.
/// Аналогично опущены <c>business_connection_id</c> и <c>entities</c> — по мере
/// появления реальных потребностей их можно будет добавить, не ломая существующее API.
/// </remarks>
/// <param name="ChatId">Идентификатор чата, в котором находится редактируемое сообщение.</param>
/// <param name="MessageId">Идентификатор редактируемого сообщения.</param>
/// <param name="Text">Новый текст сообщения, до 4096 символов после разбора форматирования.</param>
/// <param name="ParseMode">
/// Режим разбора разметки в <see cref="Text"/>: <c>Markdown</c>, <c>MarkdownV2</c>
/// или <c>HTML</c>. Если не задан — текст уходит как plain text.
/// </param>
/// <param name="LinkPreviewOptions">
/// Настройки превью ссылок в сообщении (см. <see cref="Telebot.LinkPreviewOptions"/>).
/// </param>
/// <param name="ReplyMarkup">
/// Новая инлайн-клавиатура под сообщением. В отличие от <c>sendMessage</c>,
/// Telegram принимает здесь только <see cref="Telebot.InlineKeyboardMarkup"/> —
/// обычные клавиатуры, force-reply и их удаление к отправленному сообщению
/// пришить нельзя, поэтому тип уже, а не общий <see cref="IReplyMarkup"/>.
/// Чтобы убрать существующую клавиатуру, передайте разметку с пустым массивом рядов.
/// </param>
public sealed record EditMessageTextRequestParams(
    long ChatId,
    int MessageId,
    string Text,
    string? ParseMode = null,
    LinkPreviewOptions? LinkPreviewOptions = null,
    InlineKeyboardMarkup? ReplyMarkup = null
) : TelegramRequest("editMessageText")
{
    /// <summary>
    /// Обязательные поля (chat_id, message_id, text) выдаются всегда;
    /// остальные — только если явно заданы. <see cref="LinkPreviewOptions"/>
    /// и <see cref="ReplyMarkup"/> уходят как JSON-строки: Telegram ожидает
    /// именно такое представление составных объектов в form-запросе.
    /// </summary>
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("chat_id", ChatId.ToString());
        yield return new TelegramRequestField("message_id", MessageId.ToString());
        yield return new TelegramRequestField("text", Text);

        if (ParseMode is not null)
            yield return new TelegramRequestField("parse_mode", ParseMode);
        if (LinkPreviewOptions is not null)
            yield return new TelegramRequestField("link_preview_options",
                JsonSerializer.Serialize(LinkPreviewOptions, TelebotJson.Options));
        if (ReplyMarkup is not null)
            yield return new TelegramRequestField("reply_markup", ReplyMarkup.ToJson());
    }
}