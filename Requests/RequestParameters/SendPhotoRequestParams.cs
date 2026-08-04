namespace Telebot;

/// <summary>
/// Параметры вызова метода <c>sendPhoto</c> — отправка фотографии в чат.
/// В отличие от <see cref="SendMessageRequestParams"/>, помимо скалярных полей
/// передаёт ещё и файл-фото (<see cref="Photo"/>), поэтому переопределяет
/// и <see cref="GetRequestFields"/>, и <see cref="GetRequestFiles"/>.
/// </summary>
/// <param name="ChatId">Идентификатор чата-получателя.</param>
/// <param name="Photo">
/// Источник фотографии: <see cref="InputFileWithId"/>, <see cref="InputFileWithUrl"/>
/// или <see cref="InputFileWithStream"/>. От выбранного варианта зависит,
/// будет ли использоваться multipart-транспорт.
/// </param>
/// <param name="Caption">Подпись к фото, до 1024 символов.</param>
/// <param name="ParseMode">Режим разбора разметки в <see cref="Caption"/>.</param>
/// <param name="HasSpoiler">Если <c>true</c>, изображение придёт с эффектом «спойлер».</param>
/// <param name="DisableNotification">Отправка без звука.</param>
/// <param name="ProtectContent">Запрет на пересылку и сохранение.</param>
/// <param name="ReplyToMessageId">Идентификатор сообщения, на которое отвечаем.</param>
/// <param name="AllowSendingWithoutReply">
/// Разрешает отправку, даже если цель ответа удалена.
/// </param>
public sealed record SendPhotoRequestParams(
    long ChatId,
    InputFile Photo,
    string? Caption = null,
    string? ParseMode = null,
    bool? HasSpoiler = null,
    bool? DisableNotification = null,
    bool? ProtectContent = null,
    int? ReplyToMessageId = null,
    bool? AllowSendingWithoutReply = null
) : TelegramRequest("sendPhoto")
{
    /// <summary>
    /// Возвращает скалярные параметры. Сам файл <see cref="Photo"/> в этот
    /// набор не входит — он попадает в <see cref="GetRequestFiles"/>,
    /// откуда транспорт решит, как его кодировать.
    /// </summary>
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("chat_id", ChatId.ToString());

        if (Caption is not null)
            yield return new TelegramRequestField("caption", Caption);

        if (ParseMode is not null)
            yield return new TelegramRequestField("parse_mode", ParseMode);

        if (HasSpoiler is not null)
            yield return new TelegramRequestField(
                "has_spoiler",
                HasSpoiler.Value ? "true" : "false"
            );

        if (DisableNotification is not null)
            yield return new TelegramRequestField(
                "disable_notification",
                DisableNotification.Value ? "true" : "false"
            );

        if (ProtectContent is not null)
            yield return new TelegramRequestField(
                "protect_content",
                ProtectContent.Value ? "true" : "false"
            );

        if (ReplyToMessageId is not null)
            yield return new TelegramRequestField(
                "reply_to_message_id",
                ReplyToMessageId.Value.ToString()
            );

        if (AllowSendingWithoutReply is not null)
            yield return new TelegramRequestField(
                "allow_sending_without_reply",
                AllowSendingWithoutReply.Value ? "true" : "false"
            );
    }

    /// <summary>
    /// Возвращает единственный файл с именем поля <c>photo</c> — именно так
    /// этот параметр ожидает Telegram Bot API. Если <see cref="Photo"/> является
    /// <see cref="InputFileWithStream"/>, транспорт автоматически выберет
    /// multipart-кодирование.
    /// </summary>
    public override IEnumerable<TelegramRequestFile> GetRequestFiles()
    {
        yield return new TelegramRequestFile("photo", Photo);
    }
}
