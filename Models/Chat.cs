using System.Text.Json.Serialization;

namespace Telebot.Models;

public record Chat(
    [property: JsonPropertyName("id")]
    long Id,

    [property: JsonPropertyName("type")]
    string Type,

    [property: JsonPropertyName("title")]
    string? Title,
    

    [property: JsonPropertyName("username")]
    string? Username,

    [property: JsonPropertyName("first_name")]
    string? FirstName,

    [property: JsonPropertyName("last_name")]
    string? LastName,

    [property: JsonPropertyName("is_forum")]
    bool? IsForum,

    [property: JsonPropertyName("is_direct_messages")]
    bool? IsDirectMessages
);

public record ChatMemberUpdated(
    [property: JsonPropertyName("chat")]
    Chat Chat,

    [property: JsonPropertyName("from")]
    User From
);

public record Location(
    [property: JsonPropertyName("latitude")]
    double Latitude,

    [property: JsonPropertyName("longitude")]
    double Longitude,

    [property: JsonPropertyName("horizontal_accuracy")]
    double? HorizontalAccuracy,

    [property: JsonPropertyName("live_period")]
    int? LivePeriod,

    [property: JsonPropertyName("heading")]
    int? Heading,

    [property: JsonPropertyName("proximity_alert_radius")]
    int? ProximityAlertRadius
);