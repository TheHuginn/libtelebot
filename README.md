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

## Кнопки под сообщением

Инлайн-клавиатура — двумерный массив: внешний уровень задаёт ряды, внутренний — кнопки в ряду. Ниже — сообщение с тремя кнопками в раскладке `[[A, B] [C]]`: первый ряд из двух кнопок, второй — из одной.

```csharp
using Telebot;

var bot = new Telegram("BOT_TOKEN");

var keyboard = new InlineKeyboardMarkup(new[]
{
    new[]
    {
        new InlineKeyboardButton("A", CallbackData: "choice:a"),
        new InlineKeyboardButton("B", CallbackData: "choice:b"),
    },
    new[]
    {
        new InlineKeyboardButton("C", CallbackData: "choice:c"),
    },
});

await bot.SendMessageAsync(
    new SendMessageRequestParams(
        ChatId: 123456789,
        Text: "Привет! Выбери один из вариантов",
        ReplyMarkup: keyboard
    ),
    CancellationToken.None
);
```

`CallbackData` вернётся в апдейте `callback_query`, когда пользователь нажмёт кнопку. Помимо инлайн-клавиатуры доступны `ReplyKeyboardMarkup`, `ReplyKeyboardRemove` и `ForceReply` — все реализуют `IReplyMarkup` и подставляются в то же поле `ReplyMarkup`.

### Обработка нажатий

Полный цикл — поймать нажатие, погасить крутилку на кнопке и обновить сообщение:

```csharp
foreach (var update in updates)
{
    if (update.CallbackQuery is not { } cb)
        continue;

    // Обязательно: подтверждаем приём. Без этого у пользователя на кнопке
    // продолжает крутиться индикатор загрузки до таймаута Telegram.
    await bot.AnswerCallbackQueryAsync(
        new AnswerCallbackQueryRequestParams(cb.Id, Text: "Принято"),
        CancellationToken.None
    );

    // Опционально: переписываем сообщение — фиксируем выбор и убираем кнопки.
    if (cb.Message is { } msg)
    {
        await bot.EditMessageTextAsync(
            new EditMessageTextRequestParams(
                ChatId: msg.Chat.Id,
                MessageId: msg.MessageId,
                Text: $"Ты выбрал: {cb.Data}"
            ),
            CancellationToken.None
        );
    }
}
```

Если нужно оставить текст, но снять клавиатуру — вместо `EditMessageTextAsync` вызови `EditMessageReplyMarkupAsync` с `ReplyMarkup: null`.

## Опросы

Опрос описывается вопросом и списком вариантов ответа (2–10 штук). Каждый вариант — отдельный `InputPollOption`.

```csharp
using Telebot;

var bot = new Telegram("BOT_TOKEN");

await bot.SendPollAsync(
    new SendPollRequestParams(
        ChatId: 123456789,
        Question: "Какой язык лучше для CLI-утилит?",
        Options: new[]
        {
            new InputPollOption("Go"),
            new InputPollOption("Rust"),
            new InputPollOption("C#"),
            new InputPollOption("Python"),
        }
    ),
    CancellationToken.None
);
```

Метод вернёт `Message` с уже заполненным `Poll` — оттуда можно взять `poll.Id`, если нужно потом закрыть опрос или отследить голоса.

## Кастомный адрес Bot API или таймаут

По умолчанию клиент ходит в `https://api.telegram.org` с таймаутом 30 секунд. Если нужно указать другой URL (переехавший домен, self-hosted [Bot API server](https://github.com/tdlib/telegram-bot-api), mock-сервер в тестах) или увеличить таймаут — передай `DefaultTransportOptions` в транспорт:

```csharp
using Telebot;

var baseAddress = Environment.GetEnvironmentVariable("TELEGRAM_API_URL") is { } url
    ? new Uri(url)
    : new Uri("https://api.telegram.org");

var transport = new DefaultTelegramTransport(new DefaultTransportOptions
{
    BaseAddress = baseAddress,
    Timeout = TimeSpan.FromSeconds(60),
});

var bot = new Telegram(transport, "BOT_TOKEN");
```

Дефолты заданы прямо на свойствах `DefaultTransportOptions`, поэтому в инициализаторе указывай только то, что реально меняешь.

Транспорт задуман как долгоживущий — держи один инстанс на весь процесс (или по одному на каждого бота, если ботов несколько). Пересоздание транспорта на каждый запрос ведёт к утечке сокетов; это классический подводный камень `HttpClient` в .NET, не специфика библиотеки.

Если нужно что-то посложнее (прокси, свой `HttpMessageHandler`, логирование запросов) — реализуй свой `ITelegramTransport` и передай его в `new Telegram(transport, token)`. `DefaultTransportOptions` намеренно оставлен минимальным.

## Что уже поддерживается

| Метод | Описание |
|---|---|
| `getMe` | информация о боте, проверка токена |
| `getUpdates` | long-polling новых событий |
| `sendMessage` | отправка текстовых сообщений |
| `sendPhoto` | отправка фото (file_id / URL / поток) |
| `sendPoll` | отправка опросов |
| `setWebhook` | регистрация webhook-URL |
| `editMessageText` | редактирование текста ранее отправленного сообщения |
| `editMessageReplyMarkup` | замена или снятие инлайн-клавиатуры |
| `answerCallbackQuery` | подтверждение приёма нажатия кнопки |

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