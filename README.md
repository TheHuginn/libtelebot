# Bucketlab.Telebot

[![NuGet](https://img.shields.io/nuget/v/Bucketlab.Telebot.svg)](https://www.nuget.org/packages/Bucketlab.Telebot)
[![Downloads](https://img.shields.io/nuget/dt/Bucketlab.Telebot.svg)](https://www.nuget.org/packages/Bucketlab.Telebot)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)

Тонкий типобезопасный клиент Telegram Bot API для .NET 9. Никакой магии, никакого `dynamic`, никакого DI-контейнера — только `System.Net.Http` и `System.Text.Json` под капотом.

- Методы называются как в [официальной документации](https://core.telegram.org/bots/api) (`SendMessageAsync`, `EditMessageTextAsync`, …) — учить нечего.
- Все ошибки — сетевые, HTTP, `ok=false` от Telegram — сводятся к единому `TelebotException` с полем `Code`.
- Файлы отправляются по `file_id`, URL или потоком — транспорт сам выбирает form-urlencoded или multipart.

## Установка

```bash
dotnet add package Bucketlab.Telebot
```

## Быстрый старт: эхо-бот

```csharp
using Telebot;
using Telebot.Models;

var bot = new Telegram("BOT_TOKEN");
var offset = 0L;

while (true)
{
    var updates = await bot.GetUpdatesAsync(
        new GetUpdatesRequestParams(Offset: offset, Timeout: 30),
        CancellationToken.None);

    foreach (var update in updates)
    {
        offset = update.UpdateId + 1;

        if (update.Message?.Text is not { } text) continue;

        await bot.SendMessageAsync(
            new SendMessageRequestParams(update.Message.Chat.Id, $"Echo: {text}"),
            CancellationToken.None);
    }
}
```

Всё, что нужно: токен от BotFather, цикл `getUpdates` с монотонным `offset`, отправка ответа.

## Кнопки и обработка нажатий

Инлайн-клавиатура — двумерный массив: внешний уровень задаёт ряды, внутренний — кнопки в ряду.

```csharp
var keyboard = new InlineKeyboardMarkup(new[]
{
    new[]
    {
        new InlineKeyboardButton("A", CallbackData: "choice:a"),
        new InlineKeyboardButton("B", CallbackData: "choice:b"),
    },
    new[] { new InlineKeyboardButton("C", CallbackData: "choice:c") },
});

await bot.SendMessageAsync(
    new SendMessageRequestParams(chatId, "Выбери вариант", ReplyMarkup: keyboard),
    CancellationToken.None);
```

При нажатии Telegram пришлёт апдейт `callback_query`. Обязательный шаг — подтвердить приём, иначе на кнопке продолжает крутиться индикатор:

```csharp
if (update.CallbackQuery is { } cb)
{
    await bot.AnswerCallbackQueryAsync(
        new AnswerCallbackQueryRequestParams(cb.Id, Text: "Принято"),
        CancellationToken.None);

    if (cb.Message is { } msg)
    {
        await bot.EditMessageTextAsync(
            new EditMessageTextRequestParams(msg.Chat.Id, msg.MessageId, $"Ты выбрал: {cb.Data}"),
            CancellationToken.None);
    }
}
```

Помимо `InlineKeyboardMarkup` доступны `ReplyKeyboardMarkup`, `ReplyKeyboardRemove` и `ForceReply` — все реализуют `IReplyMarkup` и подставляются в то же поле.

## Опросы

```csharp
var sent = await bot.SendPollAsync(
    new SendPollRequestParams(
        ChatId: chatId,
        Question: "Какой язык лучше для CLI-утилит?",
        Options: new[]
        {
            new InputPollOption("Go"),
            new InputPollOption("Rust"),
            new InputPollOption("C#"),
        }),
    CancellationToken.None);

// Закрыть опрос и получить финальную статистику:
var closed = await bot.StopPollAsync(
    new StopPollRequestParams(sent.Chat.Id, sent.MessageId),
    CancellationToken.None);
```

Изменение голосов в **неанонимных** опросах прилетает через `update.PollAnswer` (нужно добавить `poll_answer` в `allowed_updates`).

## Отправка фото

```csharp
// По URL — Telegram скачает сам:
await bot.SendPhotoAsync(
    new SendPhotoRequestParams(chatId, new InputFileWithUrl(new Uri("https://example.com/pic.jpg"))),
    CancellationToken.None);

// Потоком — multipart:
await using var file = File.OpenRead("photo.jpg");
await bot.SendPhotoAsync(
    new SendPhotoRequestParams(chatId, new InputFileWithStream(file, "image/jpeg", "photo.jpg")),
    CancellationToken.None);

// По file_id — если файл уже был на серверах Telegram:
await bot.SendPhotoAsync(
    new SendPhotoRequestParams(chatId, new InputFileWithId("AgACAgIAAxk...")),
    CancellationToken.None);
```

## Кастомный адрес Bot API и таймаут

По умолчанию клиент ходит в `https://api.telegram.org` с таймаутом 30 секунд. Переопределяется через `DefaultTransportOptions`:

```csharp
var transport = new DefaultTelegramTransport(new DefaultTransportOptions
{
    BaseAddress = new Uri("https://my-selfhosted-bot-api.local"),
    Timeout = TimeSpan.FromSeconds(60),
});

var bot = new Telegram(transport, "BOT_TOKEN");
```

Транспорт долгоживущий — держи один инстанс на процесс. Пересоздание на каждый запрос ведёт к утечке сокетов; это классическая грабля `HttpClient` в .NET, не специфика библиотеки.

Для проксей, своего `HttpMessageHandler` или логирования запросов реализуй свой `ITelegramTransport` — `DefaultTransportOptions` намеренно оставлен минимальным.

## Обработка ошибок

```csharp
try
{
    await bot.SendMessageAsync(new SendMessageRequestParams(chatId, "hi"), CancellationToken.None);
}
catch (TelebotException ex)
{
    Console.WriteLine($"{ex.Code}: {ex.Message}");
}
```

`ex.Code` — либо HTTP-статус (транспортный сбой), либо `error_code` от Telegram (прикладной), либо `null` для нарушений протокола.

## Что уже поддерживается

| Метод | Описание |
|---|---|
| `getMe` | информация о боте, проверка токена |
| `getUpdates` | long-polling новых событий |
| `setWebhook` | регистрация webhook-URL |
| `sendMessage` | отправка текстовых сообщений |
| `sendPhoto` | отправка фото (file_id / URL / поток) |
| `editMessageText` | редактирование текста ранее отправленного сообщения |
| `editMessageReplyMarkup` | замена или снятие инлайн-клавиатуры |
| `sendPoll` | отправка опросов |
| `stopPoll` | принудительное закрытие опроса с финальной статистикой |
| `answerCallbackQuery` | подтверждение приёма нажатия кнопки |

## Планы

Ближайшие направления развития — по мере запроса, без жёстких сроков:

- **Инлайн-режим.** Модель `InlineQuery`, поле в `Update`, `answerInlineQuery` с типом-результатом `InlineQueryResultArticle` для текстовых подсказок.
- **Скачивание файлов.** `getFile` + операция закачки в переданный `Stream` — так, чтобы файлы до 2 GB (self-hosted Bot API server) не буферизовались в памяти.
- **Медиа-методы.** `sendDocument`, `sendVideo`, `sendAudio` — по тому же контракту, что уже реализован для `sendPhoto`.
- **Форматирование в тексте.** `entities` в `sendMessage` / `editMessageText` (сейчас есть только `parse_mode`).
- **Бизнес-подключения.** `business_connection_id` в методах отправки и редактирования — когда появится реальный кейс с бизнес-аккаунтами.

Список не финальный — новые методы добавляются по мере появления реальных потребностей у пользователей библиотеки. Хочешь ускорить конкретный пункт — открывай issue с описанием сценария.

## Лицензия

MIT © Bucketlab