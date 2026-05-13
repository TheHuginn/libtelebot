# Bucketlab.Telebot

Типобезопасная библиотека для Telegram Bot API на .NET.

## Установка

```bash
dotnet add package Bucketlab.Telebot
```

---

# Создание клиента

```csharp
using Telebot;

var bot = new Telegram("TOKEN");
```

---

# GetMe

```csharp
using Telebot;
using Telebot.Models;

var bot = new Telegram("TOKEN");

var me = await bot.GetMeAsync(
    new GetMeRequestParams(),
    CancellationToken.None
);

Console.WriteLine(me.Username);
```

---

# SendMessage

```csharp
using Telebot;
using Telebot.Models;

var bot = new Telegram("TOKEN");

await bot.SendMessageAsync(
    new SendMessageRequestParams(
        ChatId: 123456789,
        Text: "Привет"
    ),
    CancellationToken.None
);
```

С parse mode:

```csharp
await bot.SendMessageAsync(
    new SendMessageRequestParams(
        ChatId: 123456789,
        Text: "<b>Hello</b>",
        ParseMode: "HTML"
    ),
    CancellationToken.None
);
```

---

# GetUpdates

```csharp
using Telebot;
using Telebot.Models;

var bot = new Telegram("TOKEN");

var offset = 0;

while (true)
{
    var updates = await bot.GetUpdatesAsync(
        new GetUpdatesRequestParams(
            Offset: offset,
            Timeout: 30
        ),
        CancellationToken.None
    );

    foreach (var update in updates)
    {
        offset = update.UpdateId + 1;

        if (update.Message is null)
            continue;

        Console.WriteLine(update.Message.Text);

        await bot.SendMessageAsync(
            new SendMessageRequestParams(
                ChatId: update.Message.Chat.Id,
                Text: $"Echo: {update.Message.Text}"
            ),
            CancellationToken.None
        );
    }
}
```

---

# SendPhoto

## Отправка по file_id

```csharp
await bot.SendPhotoAsync(
    new SendPhotoRequestParams(
        ChatId: 123456789,
        Photo: new InputFileWithId("FILE_ID")
    ),
    CancellationToken.None
);
```

---

## Отправка по URL

```csharp
await bot.SendPhotoAsync(
    new SendPhotoRequestParams(
        ChatId: 123456789,
        Photo: new InputFileWithUrl(
            new Uri("https://example.com/image.jpg")
        )
    ),
    CancellationToken.None
);
```

---

## Отправка файла потоком

```csharp
await using var stream = File.OpenRead("./photo.jpg");

await bot.SendPhotoAsync(
    new SendPhotoRequestParams(
        ChatId: 123456789,
        Photo: new InputFileWithStream(
            stream,
            "image/jpeg",
            "photo.jpg"
        ),
        Caption: "Фото"
    ),
    CancellationToken.None
);
```

---

# SetWebhook

```csharp
await bot.SetWebhookAsync(
    new SetWebhookRequestParams(
        Url: "https://example.com/webhook"
    ),
    CancellationToken.None
);
```

---

# SetWebhook с сертификатом

```csharp
await using var stream = File.OpenRead("./cert.pem");

await bot.SetWebhookAsync(
    new SetWebhookRequestParams(
        Url: "https://example.com/webhook",
        Certificate: new InputFileWithStream(
            stream,
            "application/x-pem-file",
            "cert.pem"
        )
    ),
    CancellationToken.None
);
```

---

# Обработка ошибок

```csharp
try
{
    await bot.SendMessageAsync(
        new SendMessageRequestParams(
            ChatId: 123,
            Text: "test"
        ),
        CancellationToken.None
    );
}
catch (TelebotException ex)
{
    Console.WriteLine(ex.Code);
    Console.WriteLine(ex.Message);
}
```

---

# Поддерживаемые методы

- getMe
- getUpdates
- sendMessage
- sendPhoto
- setWebhook

---

# Лицензия

MIT