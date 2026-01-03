using System.Text.Json.Serialization;

namespace Telebot.Models;

public record MaskPosition(
    [property: JsonPropertyName("point")]
    string Point,

    [property: JsonPropertyName("x_shift")]
    double XShift,

    [property: JsonPropertyName("y_shift")]
    double YShift,

    [property: JsonPropertyName("scale")]
    double Scale
);