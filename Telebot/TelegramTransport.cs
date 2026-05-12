using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Telebot;

/// <summary>
/// Представляет одно текстовое поле запроса к Telegram Bot API
/// (например, <c>chat_id=123</c> или <c>text=Hello</c>).
/// Используется для сериализации параметров в form-urlencoded
/// или multipart-форму.
/// </summary>
/// <param name="Name">Имя параметра, как его ожидает Telegram API.</param>
/// <param name="Value">Строковое значение параметра.</param>
public record TelegramRequestField(
    string Name,
    string Value
);

/// <summary>
/// Абстракция для входного файла, отправляемого в Telegram.
/// Telegram Bot API поддерживает три способа задания файла:
/// <list type="number">
///   <item><description>По <c>file_id</c> — файл уже загружен на серверы Telegram.</description></item>
///   <item><description>По URL — Telegram сам скачает файл по ссылке.</description></item>
///   <item><description>Поток байт — клиент загружает содержимое напрямую (multipart).</description></item>
/// </list>
/// Конкретные варианты реализованы наследниками
/// <see cref="InputFileWithId"/>, <see cref="InputFileWithUrl"/> и <see cref="InputFileWithStream"/>.
/// </summary>
public abstract record InputFile;

/// <summary>
/// Файл, идентифицируемый <c>file_id</c> — строкой, выданной Telegram ранее
/// (например, после предыдущей загрузки того же файла).
/// </summary>
/// <param name="Id">Значение <c>file_id</c> из Telegram.</param>
public sealed record InputFileWithId(string Id) : InputFile;

/// <summary>
/// Файл, на который указывает публичный URL.
/// Telegram сам скачает содержимое по этой ссылке.
/// </summary>
/// <param name="Url">Публично доступный URL ресурса.</param>
public sealed record InputFileWithUrl(Uri Url) : InputFile;

/// <summary>
/// Файл, передаваемый как поток байт.
/// Требует транспорта <c>multipart/form-data</c>, так как бинарные данные
/// нельзя поместить в обычный <c>application/x-www-form-urlencoded</c>.
/// </summary>
/// <param name="Stream">Поток с содержимым файла.</param>
/// <param name="ContentType">MIME-тип содержимого (например, <c>image/jpeg</c>).</param>
/// <param name="FileName">Имя файла, попадающее в заголовок <c>filename</c> части multipart.</param>
public sealed record InputFileWithStream(Stream Stream, string ContentType, string FileName) : InputFile;

/// <summary>
/// Связывает имя поля файла, как его ожидает Telegram (например, <c>photo</c>),
/// с самим источником файла (<see cref="InputFileWithId"/>,
/// <see cref="InputFileWithUrl"/> или <see cref="InputFileWithStream"/>).
/// </summary>
/// <param name="Name">Имя файлового параметра в запросе Telegram.</param>
/// <param name="File">Источник содержимого файла.</param>
public record TelegramRequestFile(
    string Name,
    InputFile File
);

/// <summary>
/// Контракт для объектов, способных описать себя как набор полей и файлов
/// запроса к Telegram. Используется транспортом для построения HTTP-сообщения.
/// </summary>
public interface ITelegramEncodable
{
    /// <summary>
    /// Возвращает все скалярные параметры запроса
    /// (строки, числа, JSON-сериализованные структуры).
    /// </summary>
    IEnumerable<TelegramRequestField> GetRequestFields();

    /// <summary>
    /// Возвращает все файловые параметры запроса (медиа, документы и т. п.).
    /// </summary>
    IEnumerable<TelegramRequestFile> GetRequestFiles();
}

/// <summary>
/// Контракт транспортного слоя: умеет выполнить запрос к Telegram Bot API
/// и десериализовать поле <c>result</c> в указанный тип-параметр метода.
/// Вынесен в интерфейс, чтобы можно было подменять реализацию в тестах
/// или подключать собственный HTTP-клиент.
/// </summary>
public interface ITelegramTransport
{
    /// <summary>
    /// Выполняет запрос к Telegram Bot API.
    /// </summary>
    /// <typeparam name="T">Ожидаемый тип значения поля <c>result</c>.</typeparam>
    /// <param name="requestParams">Описание запроса — endpoint и параметры.</param>
    /// <param name="token">Bot-токен, выданный BotFather.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Десериализованное значение поля <c>result</c>.</returns>
    /// <exception cref="TelebotException">
    /// При сетевой ошибке, ненулевом <c>error_code</c> от Telegram
    /// или нарушении протокола API.
    /// </exception>
    public Task<T> RequestAsync<T>(TelegramRequest requestParams, string token, CancellationToken cancellationToken);
}

/// <summary>
/// Реализация транспорта по умолчанию: использует один статический
/// <see cref="HttpClient"/> и общается с <c>api.telegram.org</c>
/// через POST-запросы.
/// </summary>
public class DefaultTelegramTransport : ITelegramTransport
{

    // Один общий HttpClient на всё время жизни процесса — рекомендуемая практика
    // в .NET: HttpClient безопасен для многопоточного использования и переиспользует
    // TCP-соединения. Создание нового на каждый запрос приводит к утечке сокетов.
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://api.telegram.org"),
        Timeout = TimeSpan.FromSeconds(30),
    };

    /// <summary>
    /// Внутренний DTO для разбора ответа Telegram. Ответ всегда имеет
    /// обёрточную форму: <c>{ "ok": bool, "result": ..., "error_code": ..., "description": ... }</c>.
    /// Поле <see cref="Result"/> хранится как <see cref="JsonElement"/>,
    /// потому что его тип зависит от конкретного метода API и десериализуется
    /// уже снаружи в <c>T</c>.
    /// </summary>
    private record TelegramResponse(
        bool Ok,
        int? ErrorCode = null,
        string? Description = null,
        JsonElement? Result = null
    );

    /// <summary>
    /// Строит HTTP-запрос в формате <c>application/x-www-form-urlencoded</c>.
    /// Этот формат компактнее multipart и используется, когда среди файлов
    /// нет потоков — только <c>file_id</c> и URL, которые являются обычными строками.
    /// </summary>
    /// <param name="fields">Скалярные параметры запроса.</param>
    /// <param name="files">Файловые параметры (поддерживаются только id и URL).</param>
    /// <returns>Готовое HTTP-сообщение без проставленного <c>RequestUri</c>.</returns>
    /// <exception cref="TelebotException">
    /// Если среди файлов оказался <see cref="InputFileWithStream"/>:
    /// поток нельзя передать через form-urlencoded.
    /// </exception>
    private HttpRequestMessage RequestWithFormData(
        IEnumerable<TelegramRequestField> fields,
        IEnumerable<TelegramRequestFile> files)
    {
        // Собираем все пары "имя=значение" в один список — позже передадим
        // его в FormUrlEncodedContent, который сам выполнит url-кодирование.
        var pairs = new List<KeyValuePair<string, string>>();

        // Скалярные поля кладём напрямую как есть.
        foreach (var field in fields)
            pairs.Add(new(field.Name, field.Value));

        // Файлы тоже превращаем в пары "имя=строка": для id — сам id,
        // для url — его строковое представление.
        foreach (var file in files)
        {
            switch (file.File)
            {
                case InputFileWithId id:
                    pairs.Add(new(file.Name, id.Id));
                    break;

                case InputFileWithUrl url:
                    pairs.Add(new(file.Name, url.Url.ToString()));
                    break;

                // Если внезапно среди файлов оказался поток — это ошибка
                // вызывающего: до вызова этого метода нужно было выбрать
                // multipart-ветку. Бросаем исключение, чтобы не отправить
                // некорректный запрос.
                case InputFileWithStream:
                    throw new TelebotException(
                        null,
                        $"File '{file.Name}' requires multipart/form-data transport"
                    );
            }
        }

        // RequestUri оставляем пустым — он будет проставлен снаружи в RequestAsync,
        // потому что зависит от bot-token и endpoint, известных только там.
        return new HttpRequestMessage(HttpMethod.Post, "")
        {
            Content = new FormUrlEncodedContent(pairs)
        };
    }

    /// <summary>
    /// Строит HTTP-запрос в формате <c>multipart/form-data</c>.
    /// Используется, когда нужно передать хотя бы один файл как бинарный поток:
    /// multipart позволяет смешивать текстовые поля и бинарные части в одном теле.
    /// </summary>
    /// <param name="fields">Скалярные параметры запроса.</param>
    /// <param name="files">
    /// Файловые параметры: потоки кодируются бинарно,
    /// id и URL — как обычные строковые части.
    /// </param>
    /// <returns>Готовое HTTP-сообщение без проставленного <c>RequestUri</c>.</returns>
    private HttpRequestMessage RequestWithMultipart(
        IEnumerable<TelegramRequestField> fields,
        IEnumerable<TelegramRequestFile> files)
    {
        var content = new MultipartFormDataContent();

        // Текстовые поля добавляются как StringContent — каждая часть
        // получает свой Content-Disposition с указанным именем.
        foreach (var field in fields)
            content.Add(new StringContent(field.Value), field.Name);

        foreach (var file in files)
        {
            switch (file.File)
            {
                // Поток упаковываем в StreamContent и проставляем правильный
                // Content-Type (например, image/jpeg) — иначе Telegram не сможет
                // корректно интерпретировать содержимое. Третий аргумент Add — имя
                // файла, оно попадает в заголовок filename части multipart.
                case InputFileWithStream stream:
                {
                    var streamContent = new StreamContent(stream.Stream);
                    streamContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(stream.ContentType);

                    content.Add(streamContent, file.Name, stream.FileName);
                    break;
                }

                // Внутри multipart-запроса file_id и URL передаются как
                // обычные строковые части — Telegram сам разберётся, что это
                // не бинарный файл, а ссылка на уже существующий ресурс.
                case InputFileWithId id:
                    content.Add(new StringContent(id.Id), file.Name);
                    break;

                case InputFileWithUrl url:
                    content.Add(new StringContent(url.Url.ToString()), file.Name);
                    break;
            }
        }

        return new HttpRequestMessage(HttpMethod.Post, "")
        {
            Content = content
        };
    }

    /// <summary>
    /// Главный метод транспорта: выполняет HTTP-вызов к Telegram Bot API
    /// и возвращает десериализованное поле <c>result</c> типа <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Логика метода по шагам:
    /// <list type="number">
    ///   <item><description>Выбор формата тела: multipart, если среди файлов есть поток, иначе form-urlencoded.</description></item>
    ///   <item><description>Формирование URI вида <c>/bot{token}/{endpoint}</c>.</description></item>
    ///   <item><description>Проверка HTTP-статуса ответа.</description></item>
    ///   <item><description>Разбор JSON-обёртки <c>{ ok, result, error_code, description }</c>.</description></item>
    ///   <item><description>Контроль инварианта <c>ok=true ⇒ result != null</c>.</description></item>
    ///   <item><description>Десериализация <c>result</c> в <typeparamref name="T"/>.</description></item>
    /// </list>
    /// Все ошибки — сетевые, протокольные и API — единообразно превращаются в
    /// <see cref="TelebotException"/>.
    /// </remarks>
    /// <typeparam name="T">Ожидаемый тип значения поля <c>result</c>.</typeparam>
    /// <param name="request">Описание запроса — endpoint и параметры.</param>
    /// <param name="token">Bot-токен, выданный BotFather.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Десериализованное значение поля <c>result</c>.</returns>
    /// <exception cref="TelebotException">
    /// HTTP-статус не 2xx, пустое тело ответа, <c>ok=false</c>,
    /// отсутствующий <c>result</c> при <c>ok=true</c> или сбой десериализации в <typeparamref name="T"/>.
    /// </exception>
    public async Task<T> RequestAsync<T>(TelegramRequest request, string token, CancellationToken cancellationToken)
    {
        // Материализуем поля и файлы один раз, чтобы не перечислять их повторно
        // (GetRequestFields/GetRequestFiles могут быть реализованы через yield).
        var fields = request.GetRequestFields().ToList();
        var files  = request.GetRequestFiles().ToList();

        // Выбираем формат тела запроса: multipart только если есть хотя бы один
        // поток — он обязателен для передачи бинарных данных. Иначе используем
        // более лёгкий form-urlencoded.
        var message = files.Any(f => f.File is InputFileWithStream)
            ? RequestWithMultipart(fields, files)
            : RequestWithFormData(fields, files);

        // Финальный путь запроса формата /bot<token>/<method> — стандартная
        // схема Telegram Bot API. BaseAddress уже задан в HttpClient,
        // поэтому здесь нужен только относительный URI.
        message.RequestUri = new Uri(
            $"/bot{token}/{request.Endpoint}",
            UriKind.Relative
        );

        // using гарантирует освобождение HttpResponseMessage и подлежащего сокета.
        using var response = await _httpClient.SendAsync(message, cancellationToken);

        // Шаг 1: транспортный уровень. Если HTTP-статус не 2xx, дальше нет смысла
        // парсить тело как стандартный ответ Telegram — отдаём сырой текст.
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new TelebotException(
                (int)response.StatusCode,
                $"HTTP error {(int)response.StatusCode}: {body}"
            );
        }

        // Шаг 2: парсинг JSON-обёртки. Если тело пустое — это нарушение протокола,
        // потому что Telegram при 2xx обязан вернуть валидный JSON-объект.
        var telegramResponse =
            await response.Content.ReadFromJsonAsync<TelegramResponse>()
            ?? throw new TelebotException(
                null,
                "Telegram API returned an empty response body"
            );

        // Шаг 3: ошибка прикладного уровня. Telegram при логических ошибках
        // (неверные параметры, нет прав и т.п.) возвращает 200 OK с ok=false,
        // поэтому различать такие случаи нужно именно по полю ok.
        if (!telegramResponse.Ok)
        {
            throw new TelebotException(
                telegramResponse.ErrorCode,
                telegramResponse.Description ?? "Telegram API error"
            );
        }

        // Шаг 4: инвариант протокола. При ok=true поле result обязано присутствовать.
        // Если его нет — это либо баг сервера, либо несоответствие нашей модели DTO.
        if (telegramResponse.Result is null)
        {
            throw new TelebotException(
                null,
                "Telegram API returned ok=true but result is missing"
            );
        }

        // Шаг 5: десериализация конкретного результата в типизированный T.
        // System.Text.Json может вернуть null, если JSON-значение литерально null —
        // в этом случае также бросаем, потому что вызывающий ждёт значение типа T.
        var result = telegramResponse.Result.Value.Deserialize<T>()
            ?? throw new TelebotException(
                null,
                $"Failed to deserialize Telegram result to {typeof(T).Name}"
            );

        return result;
    }
}
