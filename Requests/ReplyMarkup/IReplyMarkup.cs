using System.Text.Json;
using System.Text.Json.Serialization;

namespace Telebot;

/// <summary>
/// Общий контракт для всех вариантов поля <c>reply_markup</c> в методах отправки
/// сообщений (см. <see href="https://core.telegram.org/bots/api#sendmessage"/>).
/// </summary>
/// <remarks>
/// В Bot API <c>reply_markup</c> — это единый параметр, принимающий один из
/// четырёх типов (<see cref="InlineKeyboardMarkup"/>, <see cref="ReplyKeyboardMarkup"/>,
/// <see cref="ReplyKeyboardRemove"/>, <see cref="ForceReply"/>), а Telegram различает
/// их по наличию «маркерных» полей (<c>inline_keyboard</c>, <c>keyboard</c>,
/// <c>remove_keyboard</c>, <c>force_reply</c>). Явного тега нет — union без тега.
/// <para/>
/// Поэтому в C# мы отдаём наружу общий интерфейс с одним методом
/// <see cref="ToJson"/>: каждая реализация сама знает, как выглядит её
/// JSON-представление, а запрос-отправитель просто вкладывает результат
/// в поле формы <c>reply_markup</c>.
/// </remarks>
public interface IReplyMarkup
{
    /// <summary>
    /// Возвращает готовую JSON-строку в том виде, в котором её ожидает Telegram
    /// в поле формы <c>reply_markup</c>. Реализация обязана включить своё
    /// маркерное поле — без него Telegram не сможет распознать тип разметки.
    /// </summary>
    string ToJson();
}

/// <summary>
/// Общие настройки сериализации всех вариантов <see cref="IReplyMarkup"/>:
/// null-поля не попадают в JSON. Telegram отличает «поле не задано» от
/// «поле со значением по умолчанию», поэтому опциональные незаданные поля
/// нельзя выдавать явно — иначе можно случайно перезаписать поведение сервера.
/// </summary>
internal static class ReplyMarkupJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
