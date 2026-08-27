using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Telebot;

/// <summary>
/// Параметры <c>editMessageText</c> — редактирование текста ранее отправленного сообщения.
/// </summary>
/// <remarks>
/// Вариант вызова по <c>inline_message_id</c> не поддерживается (нет инлайн-режима в библиотеке).
/// </remarks>
/// <param name="ChatId">Идентификатор чата с редактируемым сообщением.</param>
/// <param name="MessageId">Идентификатор редактируемого сообщения.</param>
/// <param name="Text">Новый текст, до 4096 символов после разбора форматирования.</param>
/// <param name="ParseMode">Режим разбора разметки: <c>Markdown</c>, <c>MarkdownV2</c> или <c>HTML</c>.</param>
/// <param name="LinkPreviewOptions">Настройки превью ссылок.</param>
/// <param name="ReplyMarkup">
/// Новая инлайн-клавиатура. Для снятия клавиатуры передайте разметку с пустым массивом рядов.
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
    /// <inheritdoc />
    [UnconditionalSuppressMessage("AOT",
        "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "Десериализация происходит используя дружественный к AOT контекст который должен иметь все типы зарегистрированными")]
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