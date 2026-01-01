using System.Net.Http.Json;
using System.Text.Json;

namespace Telebot;

public partial class Telegram
{
    private record TelegramResponse(
        bool Ok,
        int? ErrorCode = null,
        string? Description = null,

        //Nullable which is abnormal better create a success type for schema, result and eror is not the same
        JsonElement? Result = null
    );

    private HttpRequestMessage RequestWithFormData(
        IEnumerable<TelegramRequestField> fields,
        IEnumerable<TelegramRequestFile> files)
    {
        var pairs = new List<KeyValuePair<string, string>>();
        foreach (var field in fields)
            pairs.Add(new KeyValuePair<string, string>(field.Name, field.Value));

        foreach (var file in files)
            switch (file.File)
            {
                case InputFileWithId id:
                    pairs.Add(new KeyValuePair<string, string>(file.Name, id.Id));
                    break;
                case InputFileWithUrl url:
                    pairs.Add(new KeyValuePair<string, string>(file.Name, url.Url.ToString()));
                    break;
                case InputFileWithStream stream:
                    throw new Exception($"Unsupported file type: {file.Name} requires multipart/form-data transport");
            }

        return new HttpRequestMessage
        {
            Content = new FormUrlEncodedContent(pairs),
            Method = HttpMethod.Post
        };
    }

    private HttpRequestMessage RequestWithMultipart(
        IEnumerable<TelegramRequestField> fields,
        IEnumerable<TelegramRequestFile> files)
    {
        var content = new MultipartFormDataContent();

        // обычные поля
        foreach (var field in fields)
            content.Add(
                new StringContent(field.Value),
                field.Name
            );

        // файлы / file-поля
        foreach (var file in files)
            switch (file.File)
            {
                case InputFileWithStream stream:
                {
                    var streamContent = new StreamContent(stream.Stream);
                    streamContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(stream.ContentType);

                    content.Add(
                        streamContent,
                        file.Name,
                        stream.FileName
                    );
                    break;
                }

                case InputFileWithId id:
                    throw new InvalidOperationException(
                        $"Unsupported InputFile type: {file.File.GetType().Name}"
                    );

                case InputFileWithUrl url:
                    throw new InvalidOperationException(
                        $"Unsupported InputFile type: {file.File.GetType().Name}"
                    );

                default:
                    throw new InvalidOperationException(
                        $"Unsupported InputFile type: {file.File.GetType().Name}"
                    );
            }

        return new HttpRequestMessage(HttpMethod.Post, "")
        {
            Content = content
        };
    }

    private async Task<T> RawRequestAsync<T>(TelegramRequest request)
    {
        var fields = request.GetRequestFields().ToList();
        var files = request.GetRequestFiles().ToList();

        //Now, decide on transport

        var message = files.Any(f => f.File is InputFileWithStream)
            ? RequestWithMultipart(fields, files)
            : RequestWithFormData(fields, files);

        message.RequestUri = new Uri(_httpClient.BaseAddress + request.Endpoint, UriKind.Absolute);
        Console.WriteLine(message.RequestUri.AbsoluteUri);

        var response = await _httpClient.SendAsync(message);
        //Handdle errors with exception, make a function for it. to throw with typed one. only for HTTP status
        response.EnsureSuccessStatusCode();


        //Serilize to TelegramResponse
        var telegramResponse = await response.Content.ReadFromJsonAsync<TelegramResponse>() ??
                               throw new Exception("Failed to serialize TelegramResponse, abnormal error");

        if (!telegramResponse.Ok)
        {
            throw new Exception(
                $"{telegramResponse.ErrorCode} :" +
                $"{telegramResponse.Description}"
            );
        }

        if (telegramResponse.Result == null)
            throw new Exception("Failed to deserialize TelegramResponse, abnormal error");

        var result = telegramResponse.Result.Value.Deserialize<T>() ?? throw new Exception(
            $"Failed to deserialize Telegram result to {typeof(T).Name}"
        );


        return result ?? throw new Exception("Failed to serialize, internal TG error");
    }
}