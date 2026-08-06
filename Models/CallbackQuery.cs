using System.Text.Json.Serialization;

namespace Telebot.Models;

/// <summary>
/// Нажатие на кнопку инлайн-клавиатуры
/// (см. <see href="https://core.telegram.org/bots/api#callbackquery"/>).
/// </summary>
/// <remarks>
/// Приходит внутри <see cref="Update.CallbackQuery"/> в ответ на нажатие
/// пользователем кнопки с непустым <c>callback_data</c>. Обработчик обычно
/// смотрит на <see cref="Data"/>, выполняет полезное действие и подтверждает
/// приём вызовом <c>answerCallbackQuery</c> — иначе у пользователя на кнопке
/// остаётся вертящийся индикатор загрузки.
/// </remarks>
/// <param name="Id">
/// Уникальный идентификатор запроса. Используется в <c>answerCallbackQuery</c>,
/// чтобы погасить индикатор на кнопке; действителен не дольше нескольких минут.
/// </param>
/// <param name="From">
/// Пользователь, нажавший кнопку.
/// </param>
/// <param name="Message">
/// Сообщение, к которому прикреплена нажатая кнопка. <c>null</c>, если сообщение
/// было отправлено через инлайн-режим (в этой библиотеке пока не поддерживается)
/// либо слишком старое, чтобы Telegram мог его вернуть.
/// <para/>
/// Внимание: в API это union-тип <c>MaybeInaccessibleMessage</c> — либо обычное
/// <see cref="Models.Message"/>, либо «заглушка» <c>InaccessibleMessage</c> для
/// удалённых или слишком старых сообщений, у которой заполнены только
/// <see cref="Models.Message.Chat"/> и <see cref="Models.Message.MessageId"/>.
/// Оба варианта помещаются в один тип <see cref="Models.Message"/>: все прочие
/// поля объявлены nullable и у заглушки просто окажутся <c>null</c>.
/// Отличить их друг от друга можно по документированному самим Telegram
/// признаку — у обычного сообщения <see cref="Models.Message.Date"/> — реальный
/// unix-timestamp, у заглушки поле <b>всегда</b> равно 0. Если оно 0 —
/// сообщение недоступно, и всё, что можно с ним сделать, — сослаться на
/// <c>chat.id</c> и <c>message_id</c> (например, удалить или отредактировать
/// вслепую, если у бота ещё есть на это права).
/// </param>
/// <param name="ChatInstance">
/// Идентификатор чата/пользователя, к которому привязана кнопка.
/// Полезен для игр и статистики нажатий; для обычных кнопок под сообщением
/// в чате его можно не использовать.
/// </param>
/// <param name="Data">
/// Данные из поля <c>callback_data</c> нажатой кнопки, 1–64 байта UTF-8.
/// Именно по этому значению обработчик обычно решает, что делать.
/// <c>null</c>, если кнопка была игрового типа (game button) — такие сценарии
/// эта библиотека пока не покрывает.
/// </param>
public record CallbackQuery(
    [property: JsonPropertyName("id")]
    string Id,

    [property: JsonPropertyName("from")]
    User From,

    [property: JsonPropertyName("message")]
    Message? Message,

    [property: JsonPropertyName("chat_instance")]
    string ChatInstance,

    [property: JsonPropertyName("data")]
    string? Data
);