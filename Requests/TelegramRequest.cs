namespace Telebot;

/// <summary>
/// Базовый тип для всех описаний запросов к Telegram Bot API.
/// Хранит имя метода (endpoint) и реализует <see cref="ITelegramEncodable"/>
/// пустыми коллекциями полей и файлов: наследники переопределяют только то,
/// что им действительно нужно передать.
/// </summary>
/// <remarks>
/// Архитектурное разделение: сам запрос знает <em>что</em> отправлять
/// (имя метода и параметры), а транспорт (<see cref="ITelegramTransport"/>)
/// знает <em>как</em> отправлять (form-urlencoded или multipart, HTTP-вызов,
/// разбор ответа). Это позволяет добавлять новые методы Telegram, не меняя
/// транспортный слой, и подменять транспорт в тестах, не меняя запросы.
/// </remarks>
/// <param name="Endpoint">
/// Имя метода Telegram Bot API (например, <c>sendMessage</c>),
/// которое попадает в URL запроса <c>/bot{token}/{endpoint}</c>.
/// </param>
public record TelegramRequest(string Endpoint) : ITelegramEncodable
{
    /// <summary>
    /// По умолчанию запрос не имеет скалярных параметров.
    /// Наследники переопределяют метод и через <c>yield return</c>
    /// возвращают только те поля, значения которых заданы.
    /// </summary>
    public virtual IEnumerable<TelegramRequestField> GetRequestFields()
    {
        return Enumerable.Empty<TelegramRequestField>();
    }

    /// <summary>
    /// По умолчанию запрос не передаёт файлов.
    /// Переопределяется только в тех методах, которые отправляют медиа
    /// или документы (например, <see cref="SendPhotoRequestParams"/>).
    /// </summary>
    public virtual IEnumerable<TelegramRequestFile> GetRequestFiles()
    {
        return Enumerable.Empty<TelegramRequestFile>();
    }
}