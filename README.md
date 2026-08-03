# Bucketlab.Telebot

Типобезопасная библиотека-клиент для [Telegram Bot API](https://core.telegram.org/bots/api) на .NET 9.

Вместо «универсального» вызова с произвольным именем метода и словарём параметров каждый endpoint описан отдельной моделью запроса и типизированным результатом — сигнатуру проверяет компилятор.

## Особенности

- Отдельная модель запроса на каждый endpoint (`SendMessageRequestParams`, `SendPhotoRequestParams`, …).
- Типизированные ответы: `GetMeAsync` возвращает `User`, `SendMessageAsync` — `Message` и т.д.
- Файлы можно передавать тремя способами: `InputFileWithId`, `InputFileWithUrl`, `InputFileWithStream` — транспорт сам выберет form-urlencoded или multipart.
- Единое исключение `TelebotException` для сетевых, протокольных и прикладных ошибок.
- Разделение клиента (`ITelegramClient`) и транспорта (`ITelegramTransport`) — легко подменить на мок в тестах.

## Установка

```bash
dotnet add package Bucketlab.Telebot
```

## Пример: эхо-бот

```csharp
using Telebot;
using Telebot.Models;

var bot = new Telegram("BOT_TOKEN");

var me = await bot.GetMeAsync(new GetMeRequestParams(), CancellationToken.None);
Console.WriteLine($"Started as @{me.Username}");

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

## Обработка ошибок

Все сбои (сеть, невалидный JSON, `ok: false` от Telegram) сводятся к одному исключению:

```csharp
try
{
    await bot.SendMessageAsync(
        new SendMessageRequestParams(ChatId: 123, Text: "test"),
        CancellationToken.None
    );
}
catch (TelebotException ex)
{
    // ex.Code — HTTP-статус или error_code от Telegram
    Console.WriteLine($"{ex.Code}: {ex.Message}");
}
```

## Поддерживаемые методы

- `getMe`
- `getUpdates`
- `sendMessage`
- `sendPhoto`
- `sendPoll`
- `setWebhook`

## Лицензия

MIT