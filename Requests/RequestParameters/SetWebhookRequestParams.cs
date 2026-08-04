using System.Text.Json;

namespace Telebot;

/// <summary>
/// Параметры вызова метода <c>setWebhook</c> — регистрация webhook-URL,
/// на который Telegram будет POST-ить апдейты вместо ожидания long-polling.
/// </summary>
/// <param name="Url">HTTPS-URL, на который Telegram отправит апдейты. Обязательное поле.</param>
/// <param name="Certificate">
/// Самоподписанный публичный сертификат сервера. Передаётся только как поток,
/// поскольку Telegram должен получить непосредственно содержимое PEM-файла —
/// именно поэтому тип сужен до <see cref="InputFileWithStream"/>.
/// </param>
/// <param name="IpAddress">
/// Фиксированный IP, на который Telegram будет резолвить <see cref="Url"/>
/// — помогает обойти проблемы с DNS.
/// </param>
/// <param name="MaxConnections">
/// Максимум одновременных HTTPS-соединений для доставки апдейтов (1–100,
/// по умолчанию 40). Меньшее значение снижает нагрузку на сервер.
/// </param>
/// <param name="AllowedUpdates">
/// Список интересующих типов апдейтов; сериализуется в JSON-массив.
/// </param>
/// <param name="DropPendingUpdates">
/// Если <c>true</c>, Telegram сбросит все накопленные, но ещё не доставленные
/// апдейты — полезно при перезапуске бота с чистого листа.
/// </param>
/// <param name="SecretToken">
/// Произвольная строка, которую Telegram будет присылать в заголовке
/// <c>X-Telegram-Bot-Api-Secret-Token</c> — простой способ проверить,
/// что входящий запрос действительно от Telegram, а не от стороннего источника.
/// </param>
public sealed record SetWebhookRequestParams(
    string Url,
    InputFileWithStream? Certificate = null,
    string? IpAddress = null,
    int? MaxConnections = null,
    IReadOnlyList<string>? AllowedUpdates = null,
    bool? DropPendingUpdates = null,
    string? SecretToken = null
) : TelegramRequest("setWebhook")
{
    /// <summary>
    /// Возвращает скалярные поля webhook'а. <see cref="Certificate"/>
    /// сюда не попадает — это файл, он отдаётся через <see cref="GetRequestFiles"/>.
    /// </summary>
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("url", Url);

        if (IpAddress is not null)
            yield return new TelegramRequestField("ip_address", IpAddress);

        if (MaxConnections is not null)
            yield return new TelegramRequestField(
                "max_connections",
                MaxConnections.Value.ToString()
            );

        if (AllowedUpdates is not null)
            yield return new TelegramRequestField(
                "allowed_updates",
                JsonSerializer.Serialize(AllowedUpdates)
            );

        if (DropPendingUpdates is not null)
            yield return new TelegramRequestField(
                "drop_pending_updates",
                DropPendingUpdates.Value ? "true" : "false"
            );

        if (SecretToken is not null)
            yield return new TelegramRequestField("secret_token", SecretToken);
    }

    /// <summary>
    /// Возвращает сертификат как файл с именем поля <c>certificate</c>,
    /// только если он задан. Наличие потока в файлах автоматически
    /// заставит транспорт использовать multipart/form-data.
    /// </summary>
    public override IEnumerable<TelegramRequestFile> GetRequestFiles()
    {
        if (Certificate is not null)
            yield return new TelegramRequestFile("certificate", Certificate);
    }
}
