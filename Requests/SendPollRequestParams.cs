using System.Text.Json;
using System.Text.Json.Serialization;

namespace Telebot;

/// <summary>
/// Описание одного варианта ответа в опросе для метода <c>sendPoll</c>.
/// В полной спецификации Bot API у <c>InputPollOption</c> есть ещё
/// <c>text_parse_mode</c> и <c>text_entities</c> для форматирования подписи,
/// а также медиа-варианты — здесь они намеренно опущены, чтобы оставить
/// минимально необходимый набор: только текст варианта.
/// </summary>
/// <param name="Text">
/// Текст варианта ответа, 1–100 символов. Сериализуется в поле <c>text</c>
/// JSON-объекта, как этого ожидает Telegram.
/// </param>
public sealed record InputPollOption(
    [property: JsonPropertyName("text")] string Text
);

/// <summary>
/// Параметры вызова метода <c>sendPoll</c> — отправка опроса в чат.
/// Здесь оставлены только три обязательных поля Bot API: чат-получатель,
/// текст вопроса и список вариантов ответа. Опциональные параметры
/// (анонимность, множественный выбор, режим викторины, объяснение,
/// таймеры закрытия и т.п.) намеренно опущены — их можно добавить
/// в отдельной перегрузке, не ломая базовый сценарий.
/// </summary>
/// <param name="ChatId">Идентификатор чата-получателя (пользователь, группа или канал).</param>
/// <param name="Question">Текст вопроса, 1–300 символов.</param>
/// <param name="Options">
/// Список вариантов ответа (2–10 элементов). Передаётся в API как JSON-массив
/// объектов <see cref="InputPollOption"/>, поэтому сериализуется явно через
/// <see cref="JsonSerializer"/>, а не как form-поле «через запятую».
/// </param>
public sealed record SendPollRequestParams(
    long ChatId,
    string Question,
    IReadOnlyList<InputPollOption> Options
) : TelegramRequest("sendPoll")
{
    /// <summary>
    /// Все три поля обязательны, поэтому выдаются безусловно.
    /// <c>options</c> уходит именно как JSON-массив: Telegram не принимает
    /// его в form-urlencoded виде со скалярным значением.
    /// </summary>
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("chat_id", ChatId.ToString());
        yield return new TelegramRequestField("question", Question);
        yield return new TelegramRequestField("options", JsonSerializer.Serialize(Options));
    }
}
