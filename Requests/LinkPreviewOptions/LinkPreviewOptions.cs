using System.Text.Json.Serialization;

namespace Telebot;

/// <summary>
/// Настройки генерации превью ссылок для отправляемого сообщения
/// (см. <see href="https://core.telegram.org/bots/api#linkpreviewoptions"/>).
/// </summary>
/// <remarks>
/// В Telegram Bot API этот объект передаётся как JSON-строка в поле формы
/// <c>link_preview_options</c> — сериализацию выполняет вызывающий запрос
/// (см. <see cref="SendMessageRequestParams.GetRequestFields"/>). Пришёл
/// на смену устаревшему <c>disable_web_page_preview</c> в Bot API 7.0.
/// </remarks>
/// <param name="IsDisabled">
/// Если <c>true</c>, превью ссылок в сообщении полностью отключено.
/// </param>
/// <param name="Url">
/// URL, для которого нужно построить превью. Если не задан — берётся
/// первая ссылка из текста сообщения.
/// </param>
/// <param name="PreferSmallMedia">
/// Если <c>true</c>, превью будет уменьшенным. Игнорируется, если для
/// выбранного URL медиа не может быть уменьшено.
/// </param>
/// <param name="PreferLargeMedia">
/// Если <c>true</c>, превью будет увеличенным. Игнорируется, если для
/// выбранного URL медиа не может быть увеличено.
/// </param>
/// <param name="ShowAboveText">
/// Если <c>true</c>, превью показывается над текстом сообщения; иначе — под ним.
/// </param>
public sealed record LinkPreviewOptions(
    [property: JsonPropertyName("is_disabled")]
    bool? IsDisabled = null,
    [property: JsonPropertyName("url")]
    string? Url = null,
    [property: JsonPropertyName("prefer_small_media")]
    bool? PreferSmallMedia = null,
    [property: JsonPropertyName("prefer_large_media")]
    bool? PreferLargeMedia = null,
    [property: JsonPropertyName("show_above_text")]
    bool? ShowAboveText = null
);