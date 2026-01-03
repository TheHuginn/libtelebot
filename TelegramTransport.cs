using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Telebot;

public interface ITelegramTransport
{
    //Primary method for a Telegram client to make requests
    public Task<T> RequestAsync<T>(TelegramRequest requestParams, string token);
}

public class DefaultTelegramTransport : ITelegramTransport
{
    
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://api.telegram.org"),
        Timeout = TimeSpan.FromSeconds(30),
    };
    
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

    public async Task<T> RequestAsync<T>(TelegramRequest request, string token)
    {
        var fields = request.GetRequestFields().ToList();
        var files  = request.GetRequestFiles().ToList();

        var message = files.Any(f => f.File is InputFileWithStream)
            ? RequestWithMultipart(fields, files)
            : RequestWithFormData(fields, files);

        message.RequestUri = new Uri(
            $"/bot{token}/{request.Endpoint}",
            UriKind.Relative
        );
        
        Console.WriteLine(message.RequestUri);

        using var response = await _httpClient.SendAsync(message);

        // HTTP / transport errors
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new TelebotException(
                (int)response.StatusCode,
                $"HTTP error {(int)response.StatusCode}: {body}"
            );
        }

        var telegramResponse =
            await response.Content.ReadFromJsonAsync<TelegramResponse>()
            ?? throw new TelebotException(
                null,
                "Telegram API returned an empty response body"
            );

        // Telegram API error
        if (!telegramResponse.Ok)
        {
            throw new TelebotException(
                telegramResponse.ErrorCode,
                telegramResponse.Description ?? "Telegram API error"
            );
        }

        // Protocol invariant
        if (telegramResponse.Result is null)
        {
            throw new TelebotException(
                null,
                "Telegram API returned ok=true but result is missing"
            );
        }

        var result = telegramResponse.Result.Value.Deserialize<T>()
            ?? throw new TelebotException(
                null,
                $"Failed to deserialize Telegram result to {typeof(T).Name}"
            );

        return result;
    }
}