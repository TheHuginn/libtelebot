# Bucketlab.Telebot

[![NuGet](https://img.shields.io/nuget/v/Bucketlab.Telebot.svg)](https://www.nuget.org/packages/Bucketlab.Telebot)
[![Downloads](https://img.shields.io/nuget/dt/Bucketlab.Telebot.svg)](https://www.nuget.org/packages/Bucketlab.Telebot)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)

**Bucketlab.Telebot** — простой и понятный клиент Telegram Bot API для .NET. Забираешь токен у BotFather, подключаешь пакет — и через 10 строк кода у тебя живой бот.

Библиотека делает одно и делает это хорошо: превращает вызовы Telegram Bot API в обычные C#-методы с нормальными типами. Никакой магии, никаких `dynamic`, никакой ручной сборки multipart-запросов.

## Почему Bucketlab.Telebot

- **Тонкий слой над API** — методы называются как в документации Telegram (`SendMessageAsync`, `SendPhotoAsync`, `SetWebhookAsync`), учить нечего.
- **Типобезопасно** — параметры и ответы описаны C#-типами, IDE подсказывает поля, компилятор ловит опечатки.
- **Работает с файлами из коробки** — отправляй фото по `file_id`, по URL или загружай поток, транспорт сам разберётся с multipart.
- **Одно исключение на все ошибки** — `TelebotException` с кодом, ловится в одном `catch`.
- **Ноль зависимостей сверху** — только `System.Net.Http` и `System.Text.Json`, никакого DI-контейнера, никаких обязательных фреймворков.
- **.NET 9, nullable enabled** — современный C# без легаси.

## Установка

```bash
dotnet add package Bucketlab.Telebot
```

## Быстрый старт: эхо-бот

```csharp
using Telebot;
using Telebot.Models;

var bot = new Telegram("BOT_TOKEN");
var offset = 0;

while (true)
{
    var updates = await bot.GetUpdatesAsync(
        new GetUpdatesRequestParams(Offset: offset, Timeout: 30),
        CancellationToken.None
    );

    foreach (var update in updates)
    {
        offset = update.UpdateId + 1;

        if (update.Message?.Text is not { } text)
            continue;

        await bot.SendMessageAsync(
            new SendMessageRequestParams(
                ChatId: update.Message.Chat.Id,
                Text: $"Echo: {text}"
            ),
            CancellationToken.None
        );
    }
}
```

Запусти — и бот отвечает на любое сообщение.

## Что уже поддерживается

| Метод | Описание |
|---|---|
| `getMe` | информация о боте, проверка токена |
| `getUpdates` | long-polling новых событий |
| `sendMessage` | отправка текстовых сообщений |
| `sendPhoto` | отправка фото (file_id / URL / поток) |
| `sendPoll` | отправка опросов |
| `setWebhook` | регистрация webhook-URL |

Список будет расти — методы добавляются по мере необходимости.

## Обработка ошибок

```csharp
try
{
    await bot.SendMessageAsync(
        new SendMessageRequestParams(ChatId: 123, Text: "hi"),
        CancellationToken.None
    );
}
catch (TelebotException ex)
{
    Console.WriteLine($"{ex.Code}: {ex.Message}");
}
```

## Лицензия

MIT © Bucketlab