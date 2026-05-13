using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MessagingService.Api.Data;
using MessagingService.Api.Models;
using System.Security.Claims;

namespace MessagingService.Api.Hubs;

public class ChatHub : Hub
{
    private readonly ChatDbContext _context;

    public ChatHub(ChatDbContext context)
    {
        _context = context;
    }




    public async Task SendMessage(Guid receiverId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new HubException("Сообщение не может быть пустым");

        var userIdClaim = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userNameClaim = Context.User.FindFirst(ClaimTypes.Name)?.Value;

        Guid senderId = Guid.Empty;
        string senderName = userNameClaim ?? "Unknown";

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out senderId))
        {
            // Нормально
        }
        else
        {
            senderId = await GetUserIdFromTokenAsync();
            if (senderId == Guid.Empty)
                throw new HubException("Пользователь не авторизован");
        }

        // Получаем имя получателя
        string receiverName = "Пользователь";
        try
        {
            using var httpClient = new HttpClient();
            var token = Context.GetHttpContext()?.Request.Headers["Authorization"]
                            .FirstOrDefault()?.Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"http://localhost:5002/api/users/{receiverId}");

                if (response.IsSuccessStatusCode)
                {
                    var userData = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                    receiverName = userData.TryGetProperty("name", out var nameProp)
                        ? nameProp.GetString() ?? "Пользователь"
                        : "Пользователь";
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось получить имя получателя: {ex.Message}");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            SenderName = senderName,
            ReceiverId = receiverId,
            ReceiverName = receiverName,
            Text = text.Trim(),
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var messageDto = new
        {
            message.Id,
            message.SenderId,
            message.SenderName,
            message.ReceiverId,
            message.ReceiverName,
            message.Text,
            message.SentAt
        };

        var channel = GetChannel(senderId, receiverId);
        await Clients.Group(channel).SendAsync("ReceiveMessage", messageDto);

        Console.WriteLine($"[SignalR] Сообщение отправлено: {senderId} → {receiverId} | ReceiverName: {receiverName}");
    }







    //public async Task SendMessage(Guid receiverId, string text)
    //{
    //    if (string.IsNullOrWhiteSpace(text))
    //        throw new HubException("Сообщение не может быть пустым");

    //    var userIdClaim = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //    var userNameClaim = Context.User.FindFirst(ClaimTypes.Name)?.Value;

    //    Guid senderId = Guid.Empty;
    //    string senderName = userNameClaim ?? "Unknown";

    //    if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out senderId))
    //    {
    //        // OK
    //    }
    //    else
    //    {
    //        senderId = await GetUserIdFromTokenAsync();
    //        if (senderId == Guid.Empty)
    //            throw new HubException("Пользователь не авторизован");
    //    }

    //    // === Получаем имя получателя ===
    //    string receiverName = "Пользователь";
    //    try
    //    {
    //        using var httpClient = new HttpClient();
    //        var token = Context.GetHttpContext()?.Request.Headers["Authorization"]
    //                        .FirstOrDefault()?.Replace("Bearer ", "");

    //        if (!string.IsNullOrEmpty(token))
    //        {
    //            httpClient.DefaultRequestHeaders.Authorization =
    //                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    //            var response = await httpClient.GetAsync($"http://localhost:5002/api/users/{receiverId}");
    //            if (response.IsSuccessStatusCode)
    //            {
    //                var userData = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    //                receiverName = userData.GetProperty("name").GetString() ?? "Пользователь";
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Не удалось получить имя получателя: {ex.Message}");
    //    }

    //    var message = new Message
    //    {
    //        Id = Guid.NewGuid(),
    //        SenderId = senderId,
    //        SenderName = senderName,
    //        ReceiverId = receiverId,
    //        ReceiverName = receiverName,           // ← Важно!
    //        Text = text.Trim(),
    //        SentAt = DateTime.UtcNow
    //    };

    //    _context.Messages.Add(message);
    //    await _context.SaveChangesAsync();

    //    var messageDto = new
    //    {
    //        message.Id,
    //        message.SenderId,
    //        message.SenderName,
    //        message.ReceiverId,
    //        message.ReceiverName,      // ← тоже добавь
    //        message.Text,
    //        message.SentAt
    //    };

    //    var channel = GetChannel(senderId, receiverId);
    //    await Clients.Group(channel).SendAsync("ReceiveMessage", messageDto);

    //    Console.WriteLine($"[SignalR] Сообщение отправлено: {senderId} → {receiverId}");
    //}












    //public async Task SendMessage(Guid receiverId, string text)
    //{
    //    if (string.IsNullOrWhiteSpace(text))
    //        throw new HubException("Сообщение не может быть пустым");

    //    var userIdClaim = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //    var userNameClaim = Context.User.FindFirst(ClaimTypes.Name)?.Value;

    //    Guid senderId;
    //    string senderName = userNameClaim ?? "Unknown";

    //    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out senderId))
    //    {
    //        // Fallback — если Context.User не заполнился
    //        senderId = await GetUserIdFromTokenAsync();
    //        if (senderId == Guid.Empty)
    //            throw new HubException("Пользователь не авторизован");
    //    }

    //    var message = new Message
    //    {
    //        Id = Guid.NewGuid(),
    //        SenderId = senderId,
    //        SenderName = senderName,
    //        ReceiverId = receiverId,
    //        Text = text.Trim(),
    //        SentAt = DateTime.UtcNow
    //    };

    //    // Сохраняем в базу
    //    _context.Messages.Add(message);
    //    await _context.SaveChangesAsync();

    //    // Отправляем сообщение обоим участникам
    //    var channel = GetChannel(senderId, receiverId);
    //    var messageDto = new
    //    {
    //        message.Id,
    //        message.SenderId,
    //        message.SenderName,
    //        message.ReceiverId,
    //        message.Text,
    //        message.SentAt
    //    };

    //    await Clients.Group(channel).SendAsync("ReceiveMessage", messageDto);

    //    Console.WriteLine($"[SignalR] Сообщение отправлено: {senderId} → {receiverId}");
    //}

    public async Task JoinChat(Guid otherUserId)
    {
        var userIdClaim = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out Guid userId))
        {
            userId = await GetUserIdFromTokenAsync();
            if (userId == Guid.Empty)
                throw new HubException("Unauthorized");
        }

        var groupName = GetChannel(userId, otherUserId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        Console.WriteLine($"[SignalR] Пользователь {userId} присоединился к чату {groupName}");
    }

    private async Task<Guid> GetUserIdFromTokenAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null) return Guid.Empty;

        var token = httpContext.Request.Query["access_token"].FirstOrDefault()
                    ?? httpContext.Request.Headers["Authorization"]
                        .FirstOrDefault()?.Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token)) return Guid.Empty;

        try
        {
            var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
            var secret = configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(secret)) return Guid.Empty;

            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(secret));

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out Guid userId) ? userId : Guid.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation error: {ex.Message}");
            return Guid.Empty;
        }
    }

    private static string GetChannel(Guid user1, Guid user2)
    {
        var ordered = new[] { user1, user2 }.OrderBy(x => x).ToArray();
        return $"chat_{ordered[0]}_{ordered[1]}";
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"[SignalR] Client connected: {Context.ConnectionId} | User: {Context.User?.Identity?.Name}");
        await base.OnConnectedAsync();
    }
}















//using MessagingService.Api.Data;
//using MessagingService.Api.Models;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace MessagingService.Api.Hubs;

//public class ChatHub : Hub
//{
//    private readonly ChatDbContext _context;

//    public ChatHub(ChatDbContext context)
//    {
//        _context = context;
//    }

//    public async Task SendMessage(Guid receiverId, string text)
//    {
//        if (string.IsNullOrWhiteSpace(text))
//            throw new HubException("Сообщение не может быть пустым");

//        // Получаем пользователя из Claims (рекомендуемый способ)
//        var userIdClaim = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//        var userNameClaim = Context.User.FindFirst(ClaimTypes.Name)?.Value;

//        Guid userId;
//        string userName = userNameClaim ?? "Unknown";

//        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out userId))
//        {
//            // Fallback: ручной парсинг токена (на случай, если middleware не сработал)
//            userId = await GetUserIdFromTokenAsync();
//            if (userId == Guid.Empty)
//                throw new HubException("Пользователь не авторизован");
//        }

//        var message = new Message
//        {
//            Id = Guid.NewGuid(),
//            SenderId = userId,
//            SenderName = userName,
//            ReceiverId = receiverId,
//            Text = text.Trim(),
//            SentAt = DateTime.UtcNow
//        };

//        // Сохраняем в базу
//        _context.Messages.Add(message);
//        await _context.SaveChangesAsync();

//        // Отправляем всем в группе (оба участника чата)
//        var channel = GetChannel(userId, receiverId);
//        var messageDto = new
//        {
//            message.Id,
//            message.SenderId,
//            message.SenderName,
//            message.ReceiverId,
//            message.Text,
//            message.SentAt
//        };

//        await Clients.Group(channel).SendAsync("ReceiveMessage", messageDto);

//        Console.WriteLine($"Сообщение отправлено: {userId} -> {receiverId}");
//    }

//    private async Task<Guid> GetUserIdFromTokenAsync()
//    {
//        var httpContext = Context.GetHttpContext();
//        if (httpContext == null) return Guid.Empty;

//        var token = httpContext.Request.Query["access_token"].FirstOrDefault()
//                    ?? httpContext.Request.Headers["Authorization"].FirstOrDefault()
//                        ?.Replace("Bearer ", "");

//        if (string.IsNullOrEmpty(token))
//            return Guid.Empty;

//        try
//        {
//            var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
//            var secret = configuration["Jwt:Secret"];
//            if (string.IsNullOrEmpty(secret))
//                return Guid.Empty;

//            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
//            var handler = new JwtSecurityTokenHandler();

//            var principal = handler.ValidateToken(token, new TokenValidationParameters
//            {
//                ValidateIssuerSigningKey = true,
//                IssuerSigningKey = key,
//                ValidateIssuer = false,
//                ValidateAudience = false,
//                ClockSkew = TimeSpan.Zero
//            }, out _);

//            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            return Guid.TryParse(userIdClaim, out Guid userId) ? userId : Guid.Empty;
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Token validation error: {ex.Message}");
//            return Guid.Empty;
//        }
//    }


//    // Grok сказал что это плахая реализация
//    //public async Task SendMessage(Guid receiverId, string text)
//    //{
//    //    Console.WriteLine($"SendMessage: receiverId={receiverId}, text={text}");
//    //    var httpContext = Context.GetHttpContext()!;
//    //    var userId = Guid.Empty;
//    //    var userName = "Unknown";

//    //    // Пробуем взять из QueryString (SignalR передаёт токен так)
//    //    var token = httpContext.Request.Query["access_token"].FirstOrDefault()
//    //                ?? httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

//    //    if (!string.IsNullOrEmpty(token))
//    //    {
//    //        try
//    //        {
//    //            var secret = httpContext.RequestServices.GetRequiredService<IConfiguration>()["Jwt:Secret"]!;
//    //            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
//    //            var handler = new JwtSecurityTokenHandler();
//    //            var principal = handler.ValidateToken(token, new TokenValidationParameters
//    //            {
//    //                ValidateIssuerSigningKey = true,
//    //                IssuerSigningKey = key,
//    //                ValidateIssuer = false,
//    //                ValidateAudience = false,
//    //                ClockSkew = TimeSpan.Zero
//    //            }, out _);

//    //            userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
//    //            userName = principal.FindFirst(ClaimTypes.Name)!.Value;
//    //        }
//    //        catch (Exception ex)
//    //        {
//    //            Console.WriteLine($"Token error: {ex.Message}");
//    //            throw new HubException("Unauthorized");
//    //        }
//    //    }
//    //    else
//    //    {
//    //        throw new HubException("No token");
//    //    }

//    //    var message = new Message
//    //    {
//    //        Id = Guid.NewGuid(),
//    //        SenderId = userId,
//    //        SenderName = userName,
//    //        ReceiverId = receiverId,
//    //        Text = text,
//    //        SentAt = DateTime.UtcNow
//    //    };

//    //    _context.Messages.Add(message);
//    //    await _context.SaveChangesAsync();

//    //    var channel = GetChannel(userId, receiverId);
//    //    await Clients.Group(channel).SendAsync("ReceiveMessage", new
//    //    {
//    //        message.Id,
//    //        message.SenderId,
//    //        message.SenderName,
//    //        message.ReceiverId,
//    //        message.Text,
//    //        message.SentAt
//    //    });
//    //}


//    //public async Task SendMessage(Guid receiverId, string text)
//    //{
//    //    var senderId = Middleware.JwtMiddleware.GetUserId(
//    //        Context.GetHttpContext()!
//    //    );
//    //    var senderName = Middleware.JwtMiddleware.GetUserName(
//    //        Context.GetHttpContext()!
//    //    );

//    //    var message = new Message
//    //    {
//    //        Id = Guid.NewGuid(),
//    //        SenderId = senderId,
//    //        SenderName = senderName,
//    //        ReceiverId = receiverId,
//    //        Text = text,
//    //        SentAt = DateTime.UtcNow
//    //    };

//    //    _context.Messages.Add(message);
//    //    await _context.SaveChangesAsync();

//    //    var channel = GetChannel(senderId, receiverId);
//    //    await Clients.Group(channel).SendAsync("ReceiveMessage", new
//    //    {
//    //        message.Id,
//    //        message.SenderId,
//    //        message.SenderName,
//    //        message.ReceiverId,
//    //        message.Text,
//    //        message.SentAt
//    //    });
//    //}

//    public async Task JoinChat(Guid otherUserId)
//    {
//        var userId = Middleware.JwtMiddleware.GetUserId(
//            Context.GetHttpContext()!
//        );
//        var group = GetChannel(userId, otherUserId);
//        await Groups.AddToGroupAsync(Context.ConnectionId, group);
//    }

//    private static string GetChannel(Guid user1, Guid user2)
//    {
//        var ordered = new[] { user1, user2 }.OrderBy(x => x).ToArray();
//        return $"chat_{ordered[0]}_{ordered[1]}";
//    }
//}