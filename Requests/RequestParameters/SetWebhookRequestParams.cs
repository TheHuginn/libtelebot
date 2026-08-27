using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Telebot;

/// <summary>
/// Параметры <c>setWebhook</c> — регистрация URL для доставки апдейтов.
/// </summary>
/// <param name="Url">HTTPS-URL для приёма апдейтов.</param>
/// <param name="Certificate">Самоподписанный публичный сертификат сервера (только как поток).</param>
/// <param name="IpAddress">Фиксированный IP для резолвинга <see cref="Url"/> — в обход DNS.</param>
/// <param name="MaxConnections">Максимум одновременных HTTPS-соединений (1–100, по умолчанию 40).</param>
/// <param name="AllowedUpdates">Список интересующих типов апдейтов.</param>
/// <param name="DropPendingUpdates">Сбросить накопленные, но не доставленные апдейты.</param>
/// <param name="SecretToken">
/// Секрет, который Telegram будет присылать в <c>X-Telegram-Bot-Api-Secret-Token</c>
/// — для проверки, что запрос действительно от Telegram.
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
    /// <inheritdoc />
    [UnconditionalSuppressMessage("AOT",
        "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "Десериализация происходит используя дружественный к AOT контекст который должен иметь все типы зарегистрированными")]
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
                JsonSerializer.Serialize(AllowedUpdates, TelebotJson.Options)
            );

        if (DropPendingUpdates is not null)
            yield return new TelegramRequestField(
                "drop_pending_updates",
                DropPendingUpdates.Value ? "true" : "false"
            );

        if (SecretToken is not null)
            yield return new TelegramRequestField("secret_token", SecretToken);
    }

    /// <inheritdoc />
    public override IEnumerable<TelegramRequestFile> GetRequestFiles()
    {
        if (Certificate is not null)
            yield return new TelegramRequestFile("certificate", Certificate);
    }
}