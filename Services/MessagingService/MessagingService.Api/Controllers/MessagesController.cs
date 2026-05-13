using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessagingService.Api.Data;
using MessagingService.Api.DTOs;
using MessagingService.Api.Middleware;

namespace MessagingService.Api.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly ChatDbContext _context;

    public MessagesController(ChatDbContext context)
    {
        _context = context;
    }

    //[HttpGet("chats")]
    //public async Task<ActionResult> GetMyChats()
    //{
    //    var userId = JwtMiddleware.GetUserId(HttpContext);
    //    if (userId == Guid.Empty)
    //        return Unauthorized(new { message = "Токен не предоставлен" });

    //    var chats = await _context.Messages
    //        .Where(m => m.SenderId == userId || m.ReceiverId == userId)
    //        .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
    //        .Select(g => new
    //        {
    //            UserId = g.Key,
    //            LastMessage = g.OrderByDescending(m => m.SentAt).FirstOrDefault(),
    //            MessageCount = g.Count()
    //        })
    //        .OrderByDescending(c => c.LastMessage.SentAt)
    //        .ToListAsync();

    //    // Получаем информацию о пользователях (имя + аватар)
    //    var chatList = new List<object>();

    //    foreach (var chat in chats)
    //    {
    //        // Здесь можно сделать запрос в Users Service, либо хранить имя в сообщении (как сейчас)
    //        // На первое время используем SenderName / ReceiverName из последнего сообщения
    //        var lastMsg = chat.LastMessage;

    //        chatList.Add(new
    //        {
    //            id = chat.UserId,
    //            name = lastMsg.SenderId == userId ? lastMsg.ReceiverName ?? "Пользователь"
    //                                             : lastMsg.SenderName ?? "Пользователь",
    //            // avatar = ... (пока через ui-avatars)
    //            lastMessage = lastMsg.Text,
    //            lastMessageTime = lastMsg.SentAt,
    //            unreadCount = 0 // можно потом добавить
    //        });
    //    }

    //    return Ok(new { chats = chatList });
    //}



    //[HttpGet("chats")]
    //public async Task<ActionResult> GetMyChats()
    //{
    //    var userId = JwtMiddleware.GetUserId(HttpContext);
    //    if (userId == Guid.Empty)
    //        return Unauthorized(new { message = "Токен не предоставлен" });

    //    var chats = await _context.Messages
    //        .Where(m => m.SenderId == userId || m.ReceiverId == userId)
    //        .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
    //        .Select(g => new
    //        {
    //            UserId = g.Key,
    //            LastMessage = g.OrderByDescending(m => m.SentAt).FirstOrDefault()!
    //        })
    //        .OrderByDescending(c => c.LastMessage.SentAt)
    //        .ToListAsync();

    //    var chatList = chats.Select(chat => new
    //    {
    //        id = chat.UserId,
    //        name = chat.LastMessage.SenderId == userId
    //               ? chat.LastMessage.ReceiverName
    //               : chat.LastMessage.SenderName,
    //        lastMessage = chat.LastMessage.Text,
    //        lastMessageTime = chat.LastMessage.SentAt,
    //        unreadCount = 0
    //    }).ToList();

    //    return Ok(new { chats = chatList });
    //}




    //[HttpGet("chats")]
    //public async Task<ActionResult> GetMyChats()
    //{
    //    var userId = JwtMiddleware.GetUserId(HttpContext);
    //    if (userId == Guid.Empty)
    //        return Unauthorized(new { message = "Токен не предоставлен" });

    //    // Получаем все чаты с последним сообщением
    //    var chats = await _context.Messages
    //        .Where(m => m.SenderId == userId || m.ReceiverId == userId)
    //        .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
    //        .Select(g => new
    //        {
    //            UserId = g.Key,
    //            LastMessage = g.OrderByDescending(m => m.SentAt).FirstOrDefault()
    //        })
    //        .OrderByDescending(c => c.LastMessage.SentAt)
    //        .ToListAsync();

    //    var chatList = new List<object>();

    //    foreach (var chat in chats)
    //    {
    //        var last = chat.LastMessage;
    //        if (last == null) continue;

    //        chatList.Add(new
    //        {
    //            id = chat.UserId,
    //            name = last.SenderId == userId ? last.ReceiverName : last.SenderName,
    //            lastMessage = last.Text?.Length > 50 ? last.Text.Substring(0, 47) + "..." : last.Text,
    //            lastMessageTime = last.SentAt,
    //            unreadCount = 0
    //        });
    //    }

    //    return Ok(new { chats = chatList });
    //}









    //[HttpGet("chats")]
    //public async Task<ActionResult> GetMyChats()
    //{
    //    var userId = JwtMiddleware.GetUserId(HttpContext);
    //    if (userId == Guid.Empty)
    //        return Unauthorized(new { message = "Токен не предоставлен" });

    //    // === Отладочная информация ===
    //    var allMessagesCount = await _context.Messages.CountAsync();
    //    var myMessages = await _context.Messages
    //        .Where(m => m.SenderId == userId || m.ReceiverId == userId)
    //        .OrderByDescending(m => m.SentAt)
    //        .Take(10)
    //        .ToListAsync();

    //    Console.WriteLine($"[GetMyChats] Всего сообщений в БД: {allMessagesCount}");
    //    Console.WriteLine($"[GetMyChats] Сообщений у пользователя {userId}: {myMessages.Count}");

    //    if (myMessages.Count == 0)
    //    {
    //        return Ok(new
    //        {
    //            chats = new List<object>(),
    //            debug = "Нет сообщений для этого пользователя"
    //        });
    //    }

    //    // Основная логика
    //    var chats = await _context.Messages
    //        .Where(m => m.SenderId == userId || m.ReceiverId == userId)
    //        .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
    //        .Select(g => new
    //        {
    //            UserId = g.Key,
    //            LastMessage = g.OrderByDescending(m => m.SentAt).FirstOrDefault()
    //        })
    //        .OrderByDescending(c => c.LastMessage.SentAt)
    //        .ToListAsync();

    //    var chatList = chats.Select(chat => new
    //    {
    //        id = chat.UserId,
    //        name = chat.LastMessage.SenderId == userId
    //               ? chat.LastMessage.ReceiverName ?? "Пользователь"
    //               : chat.LastMessage.SenderName ?? "Пользователь",
    //        lastMessage = chat.LastMessage.Text?.Length > 50
    //                      ? chat.LastMessage.Text.Substring(0, 47) + "..."
    //                      : chat.LastMessage.Text,
    //        lastMessageTime = chat.LastMessage.SentAt,
    //        unreadCount = 0
    //    }).ToList();

    //    return Ok(new { chats = chatList, debugTotalMessages = allMessagesCount });
    //}







    [HttpGet("chats")]
    public async Task<ActionResult> GetMyChats()
    {
        var userId = JwtMiddleware.GetUserId(HttpContext);
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "Токен не предоставлен" });

        var sql = @"
        SELECT 
            ""Id"",
            ""Name"",
            ""LastMessage"",
            ""LastMessageTime"",
            ""UnreadCount""
        FROM (
            SELECT 
                CASE 
                    WHEN ""SenderId"" = @userId THEN ""ReceiverId"" 
                    ELSE ""SenderId"" 
                END as ""Id"",

                -- Улучшенная логика получения имени
                COALESCE(
                    CASE 
                        WHEN ""SenderId"" = @userId THEN ""ReceiverName"" 
                        ELSE ""SenderName"" 
                    END,
                    'Пользователь'
                ) as ""Name"",

                ""Text"" as ""LastMessage"",
                ""SentAt"" as ""LastMessageTime"",
                0 as ""UnreadCount"",

                ROW_NUMBER() OVER (
                    PARTITION BY CASE 
                        WHEN ""SenderId"" = @userId THEN ""ReceiverId"" 
                        ELSE ""SenderId"" 
                    END 
                    ORDER BY ""SentAt"" DESC
                ) as rn
            FROM ""messages"" m
            WHERE ""SenderId"" = @userId OR ""ReceiverId"" = @userId
        ) sub
        WHERE rn = 1
        ORDER BY ""LastMessageTime"" DESC";

        var chats = await _context.Database
            .SqlQueryRaw<ChatDto>(sql, new Npgsql.NpgsqlParameter("@userId", userId))
            .ToListAsync();

        Console.WriteLine($"[GetMyChats] Найдено чатов: {chats.Count}");

        return Ok(new { chats });
    }



    ////работает, но иногда имя пользователя не вытягивает корректно
    //[HttpGet("chats")]
    //public async Task<ActionResult> GetMyChats()
    //{
    //    var userId = JwtMiddleware.GetUserId(HttpContext);
    //    if (userId == Guid.Empty)
    //        return Unauthorized(new { message = "Токен не предоставлен" });

    //    var totalMessages = await _context.Messages.CountAsync();
    //    Console.WriteLine($"[GetMyChats] Всего сообщений в БД: {totalMessages}");

    //    // Самый надёжный вариант через FromSqlRaw + правильное имя таблицы
    //    var sql = @"
    //    SELECT 
    //        ""Id"",
    //        ""Name"",
    //        ""LastMessage"",
    //        ""LastMessageTime"",
    //        ""UnreadCount""
    //    FROM (
    //        SELECT 
    //            CASE 
    //                WHEN ""SenderId"" = @userId THEN ""ReceiverId"" 
    //                ELSE ""SenderId"" 
    //            END as ""Id"",

    //            CASE 
    //                WHEN ""SenderId"" = @userId THEN COALESCE(""ReceiverName"", 'Пользователь')
    //                ELSE COALESCE(""SenderName"", 'Пользователь')
    //            END as ""Name"",

    //            ""Text"" as ""LastMessage"",
    //            ""SentAt"" as ""LastMessageTime"",
    //            0 as ""UnreadCount"",

    //            ROW_NUMBER() OVER (
    //                PARTITION BY CASE 
    //                    WHEN ""SenderId"" = @userId THEN ""ReceiverId"" 
    //                    ELSE ""SenderId"" 
    //                END 
    //                ORDER BY ""SentAt"" DESC
    //            ) as rn
    //        FROM ""messages"" m
    //        WHERE ""SenderId"" = @userId OR ""ReceiverId"" = @userId
    //    ) sub
    //    WHERE rn = 1
    //    ORDER BY ""LastMessageTime"" DESC";

    //    var chats = await _context.Database
    //        .SqlQueryRaw<ChatDto>(sql, new Npgsql.NpgsqlParameter("@userId", userId))
    //        .ToListAsync();

    //    Console.WriteLine($"[GetMyChats] Найдено чатов: {chats.Count}");

    //    return Ok(new { chats });
    //}




    [HttpGet("{otherUserId:guid}")]
    public async Task<ActionResult> GetMessages(Guid otherUserId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var userId = JwtMiddleware.GetUserId(HttpContext);
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "Токен не предоставлен" });

        var messages = await _context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId)
                     || (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderByDescending(m => m.SentAt)
            .Skip(skip)
            .Take(take)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageResponse
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderName = m.SenderName,
                ReceiverId = m.ReceiverId,
                Text = m.Text,
                SentAt = m.SentAt
            })
            .ToListAsync();

        return Ok(messages);
    }
}