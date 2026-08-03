using System.Text.Json;

namespace Telebot;

/// <summary>
/// Параметры вызова метода <c>getUpdates</c> — long-polling получение
/// новых апдейтов с серверов Telegram. Все поля опциональны: если ни одно
/// не задано, Telegram вернёт все накопленные апдейты с настройками по умолчанию.
/// </summary>
/// <param name="Offset">
/// Идентификатор первого ожидаемого апдейта. Обычно равен <c>update_id</c>
/// последнего обработанного апдейта плюс один — так Telegram понимает,
/// что предыдущие можно считать подтверждёнными и удалить.
/// </param>
/// <param name="Limit">
/// Максимальное количество апдейтов в ответе (1–100, по умолчанию 100).
/// </param>
/// <param name="Timeout">
/// Таймаут long-polling в секундах: сколько ждать на стороне Telegram,
/// если новых апдейтов нет. 0 означает короткий polling.
/// </param>
/// <param name="AllowedUpdates">
/// Список типов апдейтов, которые нас интересуют (например, <c>message</c>,
/// <c>callback_query</c>). Сериализуется в JSON-массив, как требует API.
/// </param>
public sealed record GetUpdatesRequestParams(
    int? Offset = null,
    int? Limit = null,
    int? Timeout = null,
    IReadOnlyList<string>? AllowedUpdates = null
) : TelegramRequest("GetUpdates"), ITelegramEncodable
{
    /// <summary>
    /// Возвращает только заданные параметры: Telegram отличает «параметр не задан»
    /// от «параметр со значением по умолчанию», поэтому пропускать <c>null</c>
    /// принципиально — иначе можно случайно перезаписать настройки сервера.
    /// </summary>
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        if (Offset is not null)
            yield return new TelegramRequestField("offset", Offset.Value.ToString());
        if (Limit is not null)
            yield return new TelegramRequestField("limit", Limit.Value.ToString());
        if (Timeout is not null)
            yield return new TelegramRequestField("timeout", Timeout.Value.ToString());
        // allowed_updates ожидается Telegram именно как JSON-массив строк,
        // а не как form-поле со значением вида "a,b,c", поэтому сериализуем явно.
        if (AllowedUpdates is not null)
            yield return new TelegramRequestField("allowed_updates", JsonSerializer.Serialize(AllowedUpdates));
    }
}
