using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using File = Telebot.Models.File;

namespace Telebot;

/// <summary>
/// Источник файла для загрузки в Telegram. Один из трёх вариантов:
/// <see cref="InputFileWithId"/>, <see cref="InputFileWithUrl"/>, <see cref="InputFileWithStream"/>.
/// </summary>
public abstract record InputFile;

/// <summary>Файл, идентифицируемый ранее выданным <c>file_id</c>.</summary>
/// <param name="Id">Значение <c>file_id</c> из Telegram.</param>
public sealed record InputFileWithId(string Id) : InputFile;

/// <summary>Файл по публичному URL — Telegram скачает его самостоятельно.</summary>
/// <param name="Url">Публично доступный URL.</param>
public sealed record InputFileWithUrl(Uri Url) : InputFile;

/// <summary>Файл как поток байт. Требует multipart/form-data.</summary>
/// <param name="Stream">Поток с содержимым файла.</param>
/// <param name="ContentType">MIME-тип (например, <c>image/jpeg</c>).</param>
/// <param name="FileName">Имя файла для заголовка <c>filename</c>.</param>
public sealed record InputFileWithStream(Stream Stream, string ContentType, string FileName) : InputFile;

/// <summary>Одно текстовое поле запроса (имя = значение).</summary>
public record TelegramRequestField(
    string Name,
    string Value
);

/// <summary>Файловое поле запроса: имя параметра и источник файла.</summary>
public record TelegramRequestFile(
    string Name,
    InputFile File
);

/// <summary>
/// Описание запроса в терминах полей и файлов — то, что транспорт превращает в HTTP-сообщение.
/// </summary>
public interface ITelegramEncodable
{
    /// <summary>Скалярные параметры запроса.</summary>
    IEnumerable<TelegramRequestField> GetRequestFields();

    /// <summary>Файловые параметры запроса.</summary>
    IEnumerable<TelegramRequestFile> GetRequestFiles();
}

/// <summary>
/// Транспортный слой: выполняет HTTP-вызов к Bot API и десериализует поле <c>result</c>.
/// </summary>
public interface ITelegramTransport
{
    /// <summary>
    /// Выполняет запрос к Telegram Bot API.
    /// </summary>
    /// <typeparam name="T">Ожидаемый тип поля <c>result</c>.</typeparam>
    /// <param name="requestParams">Endpoint и параметры запроса.</param>
    /// <param name="token">Bot-токен из BotFather.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Десериализованное значение поля <c>result</c>.</returns>
    /// <exception cref="TelebotException">При сетевой ошибке, <c>ok=false</c> или нарушении протокола.</exception>
    public Task<T> RequestAsync<T>(TelegramRequest requestParams, string token, CancellationToken cancellationToken);

    /// <summary>
    /// Скачивает содержимое файла с серверов Telegram.
    /// Отвечает поток над HTTP-соединением — вызывающий обязан его закрыть.
    /// </summary>
    /// <param name="file">Файл из <c>getFile</c>. Требует непустой <see cref="File.FilePath"/>.</param>
    /// <param name="token">Bot-токен из BotFather.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <exception cref="TelebotException">
    /// Если <see cref="File.FilePath"/> отсутствует, HTTP-статус не 2xx или произошла сетевая ошибка.
    /// </exception>
    public Task<Stream> DownloadAsync(File file, string token, CancellationToken cancellationToken);
}

/// <summary>
/// Настройки <see cref="DefaultTelegramTransport"/>. Полезны прежде всего для self-hosted
/// <see href="https://github.com/tdlib/telegram-bot-api">Bot API server</see>.
/// </summary>
public sealed record DefaultTransportOptions
{
    /// <summary>Таймаут одного HTTP-запроса. По умолчанию 30 секунд.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>Базовый адрес Bot API. По умолчанию <c>https://api.telegram.org</c>.</summary>
    public Uri BaseAddress { get; init; } = new("https://api.telegram.org");
}

/// <summary>
/// Реализация транспорта поверх <see cref="HttpClient"/>.
/// </summary>
/// <remarks>
/// Задумана как долгоживущий сервис — создавай один экземпляр на приложение
/// и переиспользуй. Пересоздание на каждый запрос ведёт к утечке сокетов
/// (классический подводный камень <see cref="HttpClient"/>).
/// </remarks>
public class DefaultTelegramTransport : ITelegramTransport
{
    private readonly HttpClient _httpClient;

    /// <summary>Транспорт со стандартными настройками (публичный API, таймаут 30 сек).</summary>
    public DefaultTelegramTransport() : this(new DefaultTransportOptions()) {}

    /// <summary>Транспорт с пользовательскими настройками.</summary>
    public DefaultTelegramTransport(DefaultTransportOptions options)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = options.BaseAddress,
            Timeout = options.Timeout,
        };
    }

    private record TelegramResponse(
        bool Ok,
        int? ErrorCode = null,
        string? Description = null,
        JsonElement? Result = null
    );

    private HttpRequestMessage RequestWithFormData(
        IEnumerable<TelegramRequestField> fields,
        IEnumerable<TelegramRequestFile> files)
    {
        var pairs = new List<KeyValuePair<string, string>>();

        foreach (var field in fields)
            pairs.Add(new(field.Name, field.Value));

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

                case InputFileWithStream:
                    throw new TelebotException(
                        null,
                        $"File '{file.Name}' requires multipart/form-data transport"
                    );
            }
        }

        return new HttpRequestMessage(HttpMethod.Post, "")
        {
            Content = new FormUrlEncodedContent(pairs)
        };
    }

    private HttpRequestMessage RequestWithMultipart(
        IEnumerable<TelegramRequestField> fields,
        IEnumerable<TelegramRequestFile> files)
    {
        var content = new MultipartFormDataContent();

        foreach (var field in fields)
            content.Add(new StringContent(field.Value), field.Name);

        foreach (var file in files)
        {
            switch (file.File)
            {
                case InputFileWithStream stream:
                {
                    var streamContent = new StreamContent(stream.Stream);
                    streamContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(stream.ContentType);

                    content.Add(streamContent, file.Name, stream.FileName);
                    break;
                }

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

    /// <inheritdoc />
    [UnconditionalSuppressMessage("AOT",
        "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "Десериализация происходит используя дружественный к AOT контекст который должен иметь все типы зарегистрированными")]
    public async Task<T> RequestAsync<T>(TelegramRequest request, string token, CancellationToken cancellationToken)
    {
        var fields = request.GetRequestFields().ToList();
        var files  = request.GetRequestFiles().ToList();

        // multipart нужен, только если есть хотя бы один бинарный поток
        var message = files.Any(f => f.File is InputFileWithStream)
            ? RequestWithMultipart(fields, files)
            : RequestWithFormData(fields, files);

        message.RequestUri = new Uri(
            $"/bot{token}/{request.Endpoint}",
            UriKind.Relative
        );

        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new TelebotException(
                (int)response.StatusCode,
                $"HTTP error {(int)response.StatusCode}: {body}"
            );
        }

        var telegramResponse =
            await response.Content.ReadFromJsonAsync<TelegramResponse>(TelebotJson.Options)
            ?? throw new TelebotException(
                null,
                "Telegram API returned an empty response body"
            );

        // Telegram при логических ошибках возвращает 200 OK с ok=false
        if (!telegramResponse.Ok)
        {
            throw new TelebotException(
                telegramResponse.ErrorCode,
                telegramResponse.Description ?? "Telegram API error"
            );
        }

        if (telegramResponse.Result is null)
        {
            throw new TelebotException(
                null,
                "Telegram API returned ok=true but result is missing"
            );
        }

        var result = telegramResponse.Result.Value.Deserialize<T>(TelebotJson.Options)
            ?? throw new TelebotException(
                null,
                $"Failed to deserialize Telegram result to {typeof(T).Name}"
            );

        return result;
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadAsync(File file, string token, CancellationToken cancellationToken)
    {
        if (file.FilePath is not { } filePath)
        {
            throw new TelebotException(
                null,
                $"File '{file.FileId}' has no FilePath and cannot be downloaded"
            );
        }

        var uri = new Uri($"/file/bot{token}/{filePath}", UriKind.Relative);

        try
        {
            return await _httpClient.GetStreamAsync(uri, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new TelebotException((int?)ex.StatusCode, $"HTTP error: {ex.Message}");
        }
    }
}