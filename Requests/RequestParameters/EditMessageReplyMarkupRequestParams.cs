namespace Telebot;

/// <summary>
/// Параметры вызова метода <c>editMessageReplyMarkup</c> — замена (или снятие)
/// инлайн-клавиатуры под ранее отправленным сообщением, без изменения его текста.
/// <see cref="ChatId"/> и <see cref="MessageId"/> обязательны.
/// </summary>
/// <remarks>
/// Типичный сценарий: пользователь нажал кнопку в меню, бот обработал выбор и
/// хочет убрать теперь ненужные кнопки, оставив само сообщение как есть. Для
/// «убрать клавиатуру» достаточно не передавать <see cref="ReplyMarkup"/>.
/// <para/>
/// Как и в <see cref="EditMessageTextRequestParams"/>, вариант вызова по
/// <c>inline_message_id</c> здесь не поддерживается — инлайн-режим в библиотеке
/// пока отсутствует. Опущен и <c>business_connection_id</c>: появится, когда
/// в реквестах появятся бизнес-сценарии.
/// </remarks>
/// <param name="ChatId">Идентификатор чата, в котором находится редактируемое сообщение.</param>
/// <param name="MessageId">Идентификатор редактируемого сообщения.</param>
/// <param name="ReplyMarkup">
/// Новая инлайн-клавиатура. Тип уже общего <see cref="IReplyMarkup"/>: Telegram
/// принимает здесь только <see cref="Telebot.InlineKeyboardMarkup"/>. Если
/// оставить <c>null</c> — клавиатура снимется с сообщения.
/// </param>
public sealed record EditMessageReplyMarkupRequestParams(
    long ChatId,
    int MessageId,
    InlineKeyboardMarkup? ReplyMarkup = null
) : TelegramRequest("editMessageReplyMarkup")
{
    /// <summary>
    /// Обязательные <c>chat_id</c> и <c>message_id</c> выдаются всегда;
    /// <see cref="ReplyMarkup"/> уходит как JSON-строка — Telegram ожидает
    /// именно такое представление составного объекта в form-запросе.
    /// </summary>
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("chat_id", ChatId.ToString());
        yield return new TelegramRequestField("message_id", MessageId.ToString());

        if (ReplyMarkup is not null)
            yield return new TelegramRequestField("reply_markup", ReplyMarkup.ToJson());
    }
}