namespace Telebot;

/// <summary>
/// Параметры вызова метода <c>getMe</c> — простейший запрос без аргументов,
/// возвращающий информацию о самом боте. Используется для проверки
/// валидности токена и доступности API.
/// </summary>
public sealed record GetMeRequestParams() : TelegramRequest("GetMe"), ITelegramEncodable;
