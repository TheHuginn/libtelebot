using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Telebot;

/// <summary>
/// Параметры <c>sendMessage</c> — отправка текстового сообщения.
/// </summary>
/// <param name="ChatId">Идентификатор чата-получателя.</param>
/// <param name="Text">Текст сообщения, до 4096 символов после разбора форматирования.</param>
/// <param name="MessageThreadId">Идентификатор треда форума в супергруппе с темами.</param>
/// <param name="ParseMode">Режим разбора разметки: <c>Markdown</c>, <c>MarkdownV2</c> или <c>HTML</c>.</param>
/// <param name="LinkPreviewOptions">Настройки превью ссылок.</param>
/// <param name="DisableNotification">Отправка без звука.</param>
/// <param name="ProtectContent">Запрет пересылки и сохранения.</param>
/// <param name="ReplyParameters">Параметры ответа на другое сообщение.</param>
/// <param name="ReplyMarkup">
/// Разметка под сообщением: инлайн-клавиатура, обычная клавиатура, её удаление или force-reply.
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
    /// <inheritdoc />
    [UnconditionalSuppressMessage("AOT",
        "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "Десериализация происходит используя дружественный к AOT контекст который должен иметь все типы зарегистрированными")]
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