namespace Telebot;

/// <summary>
/// Параметры вызова метода <c>stopPoll</c> — принудительное закрытие опроса,
/// ранее отправленного ботом. После вызова опрос переходит в состояние
/// <see cref="Telebot.Models.Poll.IsClosed"/> = <c>true</c>, голосовать в нём больше нельзя,
/// а всем клиентам становится виден правильный ответ (для викторин).
/// </summary>
/// <remarks>
/// Bot API идентифицирует опрос **не** по <see cref="Telebot.Models.Poll.Id"/>, а по паре
/// <see cref="ChatId"/> + <see cref="MessageId"/> сообщения, в котором он был отправлен.
/// Именно поэтому этот метод возвращает уже закрытый <see cref="Telebot.Models.Poll"/>
/// — со всей финальной статистикой голосов.
/// <para/>
/// Опущены <c>business_connection_id</c> (появится вместе с бизнес-сценариями в других
/// запросах) и вариант с <c>inline_message_id</c> — инлайн-режим библиотека пока
/// не поддерживает.
/// </remarks>
/// <param name="ChatId">Идентификатор чата, в котором находится опрос.</param>
/// <param name="MessageId">Идентификатор сообщения с опросом.</param>
/// <param name="ReplyMarkup">
/// Новая инлайн-клавиатура под сообщением-опросом. Тип уже общего <see cref="IReplyMarkup"/>:
/// Telegram здесь принимает только <see cref="Telebot.InlineKeyboardMarkup"/>. Если оставить
/// <c>null</c> — существующая клавиатура снимется с сообщения.
/// </param>
public sealed record StopPollRequestParams(
    long ChatId,
    int MessageId,
    InlineKeyboardMarkup? ReplyMarkup = null
) : TelegramRequest("stopPoll")
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