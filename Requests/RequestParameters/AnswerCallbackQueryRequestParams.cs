namespace Telebot;

/// <summary>
/// Параметры вызова метода <c>answerCallbackQuery</c> — подтверждение приёма
/// callback-запроса, пришедшего от нажатия кнопки инлайн-клавиатуры.
/// </summary>
/// <remarks>
/// Этот вызов обязателен: без него у пользователя на нажатой кнопке продолжает
/// крутиться индикатор загрузки — до тех пор, пока Telegram сам не сдастся
/// по таймауту. Даже если делать больше нечего, стоит вызвать метод с одним
/// <see cref="CallbackQueryId"/>, чтобы клиент погасил индикатор.
/// <para/>
/// Опциональные <see cref="Text"/> и <see cref="ShowAlert"/> задают, увидит ли
/// пользователь короткий тост в верхней части экрана или модальное окно.
/// </remarks>
/// <param name="CallbackQueryId">
/// Идентификатор callback-запроса из <see cref="Telebot.Models.CallbackQuery.Id"/>.
/// </param>
/// <param name="Text">
/// Текст короткого уведомления, 0–200 символов. Если <c>null</c>,
/// пользователю ничего не показывается — индикатор просто гаснет.
/// </param>
/// <param name="ShowAlert">
/// Если <c>true</c>, вместо тоста показывается модальное окно, требующее
/// подтверждения. По умолчанию — тост.
/// </param>
/// <param name="Url">
/// URL, который откроется в клиенте пользователя. Применяется в двух сценариях:
/// для игровых кнопок (в этой библиотеке не поддерживаются) и для редиректа
/// в самого бота с параметром через <c>t.me/&lt;bot&gt;?start=…</c>.
/// </param>
/// <param name="CacheTime">
/// Максимальное время в секундах, в течение которого клиент может кешировать
/// результат этого callback-запроса. По умолчанию 0 — без кеширования.
/// </param>
public sealed record AnswerCallbackQueryRequestParams(
    string CallbackQueryId,
    string? Text = null,
    bool? ShowAlert = null,
    string? Url = null,
    int? CacheTime = null
) : TelegramRequest("answerCallbackQuery")
{
    /// <summary>
    /// Обязателен только <c>callback_query_id</c>; остальные поля уходят,
    /// лишь если явно заданы. Булевы значения сериализуются как литералы
    /// <c>"true"</c>/<c>"false"</c>, как этого требует Bot API.
    /// </summary>
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("callback_query_id", CallbackQueryId);

        if (Text is not null)
            yield return new TelegramRequestField("text", Text);
        if (ShowAlert is not null)
            yield return new TelegramRequestField("show_alert",
                ShowAlert.Value ? "true" : "false");
        if (Url is not null)
            yield return new TelegramRequestField("url", Url);
        if (CacheTime is not null)
            yield return new TelegramRequestField("cache_time", CacheTime.Value.ToString());
    }
}