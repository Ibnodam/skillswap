using MeetingService.Api.Data;
using MeetingService.Api.DTOs;
using MeetingService.Api.Middleware;
using MeetingService.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MeetingService.Api.Controllers;

[ApiController]
[Route("api/meetings")]
public class MeetingsController : ControllerBase
{
    private readonly MeetingDbContext _context;

    public MeetingsController(MeetingDbContext context)
    {
        _context = context;
    }

    [HttpGet("my")]
    public async Task<ActionResult> GetMyMeetings()
    {
        var userId = JwtMiddleware.GetUserId(HttpContext);
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "Токен не предоставлен" });

        var meetings = await _context.Meetings
            .Where(m => m.RequesterId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.Date)
            .ThenByDescending(m => m.CreatedAt)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;

        var upcoming = meetings.Where(m =>
            m.Date >= today && m.Status != "cancelled" && m.Status != "completed").ToList();

        var past = meetings.Where(m =>
            (m.Date < today || m.Status == "completed") && m.Status != "cancelled").ToList();

        var cancelled = meetings.Where(m => m.Status == "cancelled").ToList();

        return Ok(new { upcoming, past, cancelled });
    }



    //// GET /api/meetings/my
    //[HttpGet("my")]
    //public async Task<ActionResult> GetMyMeetings()
    //{
    //    var userId = JwtMiddleware.GetUserId(HttpContext);
    //    if (userId == Guid.Empty)
    //        return Unauthorized(new { message = "Токен не предоставлен" });

    //    var meetings = await _context.Meetings
    //        .Where(m => m.RequesterId == userId || m.ReceiverId == userId)
    //        .OrderByDescending(m => m.Date)
    //        .ToListAsync();

    //    var today = DateTime.UtcNow.Date;

    //    var upcoming = meetings.Where(m => m.Date >= today && m.Status != "cancelled" && m.Status != "completed").ToList();
    //    var past = meetings.Where(m => m.Date < today || m.Status == "completed" || m.Status == "cancelled").ToList();

    //    return Ok(new { upcoming, past });
    //}

    [HttpGet("calendar")]
    public async Task<ActionResult> GetCalendar([FromQuery] int year, [FromQuery] int month)
    {
        var userId = JwtMiddleware.GetUserId(HttpContext);
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "Токен не предоставлен" });

        var meetings = await _context.Meetings
            .Where(m => (m.RequesterId == userId || m.ReceiverId == userId)
                        && m.Date.Year == year && m.Date.Month == month)
            .ToListAsync();

        var calendar = new Dictionary<int, List<object>>();
        foreach (var m in meetings)
        {
            var day = m.Date.Day;
            if (!calendar.ContainsKey(day))
                calendar[day] = new List<object>();

            calendar[day].Add(new
            {
                m.Id,
                m.RequesterId,
                m.RequesterName,
                m.ReceiverId,
                m.ReceiverName,
                m.SkillName,
                m.Date,
                m.Time,
                m.Duration,
                m.Format,
                m.Topic,
                m.Status
            });
        }

        return Ok(calendar);
    }


    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateMeetingRequest request)
    {
        try
        {
            var userId = JwtMiddleware.GetUserId(HttpContext);
            var userName = JwtMiddleware.GetUserName(HttpContext);

            if (userId == Guid.Empty)
                return Unauthorized(new { message = "Токен не предоставлен" });

            if (request.ReceiverId == Guid.Empty || request.SkillId == Guid.Empty)
                return BadRequest(new { message = "ReceiverId и SkillId обязательны" });

            if (userId == request.ReceiverId)
                return BadRequest(new { message = "Нельзя создать встречу с самим собой" });

            // === Получаем настоящее имя получателя ===
            string receiverName = "Пользователь"; // fallback

            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                        Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", ""));

                var response = await httpClient.GetAsync($"http://localhost:5002/api/users/{request.ReceiverId}");

                if (response.IsSuccessStatusCode)
                {
                    var userData = await response.Content.ReadFromJsonAsync<JsonElement>();
                    receiverName = userData.GetProperty("name").GetString() ?? "Пользователь";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось получить имя пользователя: {ex.Message}");
                // остаётся fallback "Пользователь"
            }

            var meeting = new Meeting
            {
                Id = Guid.NewGuid(),
                RequesterId = userId,
                RequesterName = string.IsNullOrEmpty(userName) ? "Неизвестный пользователь" : userName,
                ReceiverId = request.ReceiverId,
                ReceiverName = receiverName,                    // ← Теперь реальное имя
                SkillId = request.SkillId,
                SkillName = request.SkillName ?? "",
                Date = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc),
                Time = request.Time ?? "00:00",
                Duration = request.Duration > 0 ? request.Duration : 60,
                Format = request.Format ?? "online",
                Topic = request.Topic?.Trim(),
                Comment = request.Comment?.Trim(),
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Встреча успешно создана",
                meetingId = meeting.Id
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateMeeting] Ошибка: {ex.Message}");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }


    // Работало, но в карточке имя пользователя не отображалось :(

    //[HttpPost]
    //public async Task<ActionResult> Create([FromBody] CreateMeetingRequest request)
    //{
    //    try
    //    {
    //        var userId = JwtMiddleware.GetUserId(HttpContext);
    //        var userName = JwtMiddleware.GetUserName(HttpContext);

    //        Console.WriteLine($"[CreateMeeting] UserId: {userId}");
    //        Console.WriteLine($"[CreateMeeting] ReceiverId: {request.ReceiverId}");
    //        Console.WriteLine($"[CreateMeeting] SkillId: {request.SkillId}");
    //        Console.WriteLine($"[CreateMeeting] Date from request: {request.Date} (Kind: {request.Date.Kind})");

    //        if (userId == Guid.Empty)
    //            return Unauthorized(new { message = "Токен не предоставлен" });

    //        if (request.ReceiverId == Guid.Empty || request.SkillId == Guid.Empty)
    //            return BadRequest(new { message = "ReceiverId и SkillId обязательны" });

    //        if (userId == request.ReceiverId)
    //            return BadRequest(new { message = "Нельзя создать встречу с самим собой" });

    //        var meeting = new Meeting
    //        {
    //            Id = Guid.NewGuid(),
    //            RequesterId = userId,
    //            RequesterName = string.IsNullOrEmpty(userName) ? "Неизвестный пользователь" : userName,
    //            ReceiverId = request.ReceiverId,
    //            ReceiverName = "Пользователь",
    //            SkillId = request.SkillId,
    //            SkillName = request.SkillName ?? "",

    //            // === ИСПРАВЛЕНИЕ ===
    //            Date = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc),  // ← Важно!
    //            Time = request.Time ?? "00:00",

    //            Duration = request.Duration > 0 ? request.Duration : 60,
    //            Format = request.Format ?? "online",
    //            Topic = request.Topic?.Trim(),
    //            Comment = request.Comment?.Trim(),
    //            Status = "pending",
    //            CreatedAt = DateTime.UtcNow
    //        };

    //        _context.Meetings.Add(meeting);
    //        await _context.SaveChangesAsync();

    //        Console.WriteLine($"[CreateMeeting] Успешно создана встреча {meeting.Id}");

    //        return Ok(new
    //        {
    //            message = "Встреча успешно создана",
    //            meetingId = meeting.Id
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"[CreateMeeting] КРИТИЧНАЯ ОШИБКА: {ex.Message}");
    //        if (ex.InnerException != null)
    //            Console.WriteLine($"Inner: {ex.InnerException.Message}");

    //        Console.WriteLine(ex.StackTrace);

    //        return StatusCode(500, new
    //        {
    //            message = "Внутренняя ошибка сервера",
    //            error = ex.Message,
    //            innerError = ex.InnerException?.Message
    //        });
    //    }
    //}





    //// ==================== СОЗДАНИЕ ВСТРЕЧИ ====================
    //[HttpPost]
    //public async Task<ActionResult> Create([FromBody] CreateMeetingRequest request)
    //{
    //    try
    //    {
    //        var userId = JwtMiddleware.GetUserId(HttpContext);
    //        var userName = JwtMiddleware.GetUserName(HttpContext);

    //        Console.WriteLine($"[CreateMeeting] UserId: {userId}");
    //        Console.WriteLine($"[CreateMeeting] ReceiverId: {request.ReceiverId}");
    //        Console.WriteLine($"[CreateMeeting] SkillId: {request.SkillId}");
    //        Console.WriteLine($"[CreateMeeting] Date: {request.Date}, Time: {request.Time}");

    //        if (userId == Guid.Empty)
    //            return Unauthorized(new { message = "Токен не предоставлен" });

    //        if (request.ReceiverId == Guid.Empty || request.SkillId == Guid.Empty)
    //            return BadRequest(new { message = "ReceiverId и SkillId обязательны" });

    //        if (userId == request.ReceiverId)
    //            return BadRequest(new { message = "Нельзя создать встречу с самим собой" });

    //        var meeting = new Meeting
    //        {
    //            Id = Guid.NewGuid(),
    //            RequesterId = userId,
    //            RequesterName = string.IsNullOrEmpty(userName) ? "Неизвестный пользователь" : userName,
    //            ReceiverId = request.ReceiverId,
    //            ReceiverName = "Пользователь",                    // TODO: позже брать реальное имя
    //            SkillId = request.SkillId,
    //            SkillName = request.SkillName ?? "",
    //            Date = request.Date,                              // DateTime
    //            Time = request.Time ?? "00:00",                   // string
    //            Duration = request.Duration > 0 ? request.Duration : 60,
    //            Format = request.Format ?? "online",
    //            Topic = request.Topic,
    //            Comment = request.Comment,
    //            Status = "pending",
    //            CreatedAt = DateTime.UtcNow
    //        };

    //        _context.Meetings.Add(meeting);
    //        await _context.SaveChangesAsync();

    //        Console.WriteLine($"[CreateMeeting] Успешно создана встреча {meeting.Id}");

    //        return Ok(new
    //        {
    //            message = "Встреча успешно создана",
    //            meetingId = meeting.Id
    //        });
    //    }

    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"[CreateMeeting] КРИТИЧНАЯ ОШИБКА: {ex.Message}");
    //        Console.WriteLine($"[CreateMeeting] StackTrace: {ex.StackTrace}");

    //        if (ex.InnerException != null)
    //            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");

    //        return StatusCode(500, new
    //        {
    //            message = "Внутренняя ошибка сервера",
    //            error = ex.Message,
    //            innerError = ex.InnerException?.Message
    //        });
    //    }



    //    //catch (Exception ex)
    //    //{
    //    //    Console.WriteLine($"[CreateMeeting] КРИТИЧНАЯ ОШИБКА: {ex.Message}");
    //    //    Console.WriteLine(ex.StackTrace);
    //    //    return StatusCode(500, new { message = "Внутренняя ошибка сервера", error = ex.Message });
    //    //}
    //}

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var meeting = await _context.Meetings.FindAsync(id);
        if (meeting is null)
            return NotFound(new { message = "Встреча не найдена" });

        return Ok(meeting);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var userId = JwtMiddleware.GetUserId(HttpContext);
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "Токен не предоставлен" });

        var meeting = await _context.Meetings.FindAsync(id);
        if (meeting is null)
            return NotFound(new { message = "Встреча не найдена" });

        if (meeting.ReceiverId != userId && meeting.RequesterId != userId)
            return StatusCode(403, new { message = "Нет доступа" });

        meeting.Status = request.Status;
        await _context.SaveChangesAsync();

        return Ok(meeting);
    }

    [HttpPatch("update-past")]
    public async Task<ActionResult> UpdatePast()
    {
        var today = DateTime.UtcNow.Date;
        var pastMeetings = await _context.Meetings
            .Where(m => m.Date < today && m.Status != "completed" && m.Status != "cancelled")
            .ToListAsync();

        foreach (var m in pastMeetings)
            m.Status = "completed";

        await _context.SaveChangesAsync();

        return Ok(new { updated = pastMeetings.Count });
    }
}













//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using MeetingService.Api.Data;
//using MeetingService.Api.DTOs;
//using MeetingService.Api.Middleware;
//using MeetingService.Api.Models;

//namespace MeetingService.Api.Controllers;

//[ApiController]
//[Route("api/meetings")]
//public class MeetingsController : ControllerBase
//{
//    private readonly MeetingDbContext _context;

//    public MeetingsController(MeetingDbContext context)
//    {
//        _context = context;
//    }

//    // GET /api/meetings/my
//    [HttpGet("my")]
//    public async Task<ActionResult> GetMyMeetings()
//    {
//        var userId = JwtMiddleware.GetUserId(HttpContext);
//        if (userId == Guid.Empty)
//            return Unauthorized(new { message = "Токен не предоставлен" });

//        var meetings = await _context.Meetings
//            .Where(m => m.RequesterId == userId || m.ReceiverId == userId)
//            .OrderByDescending(m => m.Date)
//            .ToListAsync();

//        var today = DateTime.UtcNow.Date;

//        var upcoming = meetings.Where(m => m.Date >= today && m.Status != "cancelled" && m.Status != "completed").ToList();
//        var past = meetings.Where(m => m.Date < today || m.Status == "completed" || m.Status == "cancelled").ToList();

//        return Ok(new { upcoming, past });
//    }

//    // GET /api/meetings/calendar?year=2026&month=5
//    //[HttpGet("calendar")]
//    //public async Task<ActionResult> GetCalendar([FromQuery] int year, [FromQuery] int month)
//    //{
//    //    var userId = JwtMiddleware.GetUserId(HttpContext);
//    //    if (userId == Guid.Empty)
//    //        return Unauthorized(new { message = "Токен не предоставлен" });

//    //    var daysInMonth = DateTime.DaysInMonth(year, month);
//    //    var startDate = new DateTime(year, month, 1);
//    //    var endDate = new DateTime(year, month, daysInMonth);

//    //    var meetings = await _context.Meetings
//    //        .Where(m => (m.RequesterId == userId || m.ReceiverId == userId)
//    //                    && m.Date >= startDate && m.Date <= endDate)
//    //        .ToListAsync();

//    //    var calendar = new Dictionary<int, List<MeetingResponse>>();
//    //    foreach (var m in meetings)
//    //    {
//    //        var day = m.Date.Day;
//    //        if (!calendar.ContainsKey(day))
//    //            calendar[day] = new List<MeetingResponse>();

//    //        calendar[day].Add(new MeetingResponse
//    //        {
//    //            Id = m.Id,
//    //            RequesterId = m.RequesterId,
//    //            RequesterName = m.RequesterName,
//    //            ReceiverId = m.ReceiverId,
//    //            ReceiverName = m.ReceiverName,
//    //            SkillId = m.SkillId,
//    //            SkillName = m.SkillName,
//    //            Date = m.Date,
//    //            Time = m.Time,
//    //            Duration = m.Duration,
//    //            Format = m.Format,
//    //            Topic = m.Topic,
//    //            Status = m.Status
//    //        });
//    //    }

//    //    return Ok(calendar);
//    //}



//    [HttpGet("calendar")]
//    public async Task<ActionResult> GetCalendar([FromQuery] int year, [FromQuery] int month)
//    {
//        var userId = JwtMiddleware.GetUserId(HttpContext);
//        if (userId == Guid.Empty)
//            return Unauthorized(new { message = "Токен не предоставлен" });

//        var meetings = await _context.Meetings
//            .Where(m => (m.RequesterId == userId || m.ReceiverId == userId)
//                        && m.Date.Year == year && m.Date.Month == month)
//            .ToListAsync();

//        var calendar = new Dictionary<int, List<object>>();
//        foreach (var m in meetings)
//        {
//            var day = m.Date.Day;
//            if (!calendar.ContainsKey(day))
//                calendar[day] = new List<object>();

//            calendar[day].Add(new
//            {
//                m.Id,
//                m.RequesterId,
//                m.RequesterName,
//                m.ReceiverId,
//                m.ReceiverName,
//                m.SkillName,
//                m.Date,
//                m.Time,
//                m.Duration,
//                m.Format,
//                m.Topic,
//                m.Status
//            });
//        }

//        return Ok(calendar);
//    }



//    [HttpPost]
//    public async Task<ActionResult> Create([FromBody] CreateMeetingRequest request)
//    {
//        try
//        {
//            var userId = JwtMiddleware.GetUserId(HttpContext);
//            var userName = JwtMiddleware.GetUserName(HttpContext);

//            Console.WriteLine($"[CreateMeeting] UserId: {userId}");
//            Console.WriteLine($"[CreateMeeting] ReceiverId: {request.ReceiverId}");
//            Console.WriteLine($"[CreateMeeting] SkillId: {request.SkillId}");
//            Console.WriteLine($"[CreateMeeting] Date: {request.Date}, Time: {request.Time}");

//            if (userId == Guid.Empty)
//                return Unauthorized(new { message = "Токен не предоставлен" });

//            if (request.ReceiverId == Guid.Empty || request.SkillId == Guid.Empty)
//                return BadRequest(new { message = "ReceiverId и SkillId обязательны" });

//            var meeting = new Meeting
//            {
//                Id = Guid.NewGuid(),
//                RequesterId = userId,
//                RequesterName = userName ?? "Неизвестный",
//                ReceiverId = request.ReceiverId,
//                ReceiverName = "Пользователь", // TODO: позже подтягивать из Users сервиса
//                SkillId = request.SkillId,
//                SkillName = request.SkillName ?? "",
//                Date = request.Date,
//                Time = request.Time ?? "00:00",
//                Duration = request.Duration,
//                Format = request.Format,
//                Topic = request.Topic,
//                Comment = request.Comment,
//                Status = "pending",
//                CreatedAt = DateTime.UtcNow
//            };

//            _context.Meetings.Add(meeting);
//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                message = "Встреча успешно создана",
//                meetingId = meeting.Id
//            });
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"[CreateMeeting] КРИТИЧНАЯ ОШИБКА: {ex.Message}");
//            Console.WriteLine(ex.StackTrace);
//            return StatusCode(500, new { message = "Внутренняя ошибка сервера", error = ex.Message });
//        }
//    }





//    //[HttpPost]
//    //public async Task<ActionResult> Create([FromBody] CreateMeetingRequest request)
//    //{
//    //    try
//    //    {
//    //        var userId = JwtMiddleware.GetUserId(HttpContext);
//    //        var userName = JwtMiddleware.GetUserName(HttpContext);

//    //        Console.WriteLine($"[CreateMeeting] UserId from token: {userId}");
//    //        Console.WriteLine($"[CreateMeeting] UserName from token: {userName}");
//    //        Console.WriteLine($"[CreateMeeting] Payload: ReceiverId={request.ReceiverId}, SkillId={request.SkillId}, Date={request.Date}");

//    //        if (userId == Guid.Empty)
//    //            return Unauthorized(new { message = "Токен не предоставлен или недействителен" });

//    //        if (request.ReceiverId == Guid.Empty || request.SkillId == Guid.Empty)
//    //            return BadRequest(new { message = "ReceiverId и SkillId обязательны" });

//    //        if (userId == request.ReceiverId)
//    //            return BadRequest(new { message = "Нельзя создать встречу с самим собой" });

//    //        var meeting = new Meeting
//    //        {
//    //            Id = Guid.NewGuid(),
//    //            RequesterId = userId,
//    //            RequesterName = string.IsNullOrEmpty(userName) ? "Неизвестный пользователь" : userName,
//    //            ReceiverId = request.ReceiverId,
//    //            ReceiverName = "User",                    // TODO: потом исправить
//    //            SkillId = request.SkillId,
//    //            SkillName = request.SkillName ?? "",
//    //            Date = request.Date.Date,                 // убираем время
//    //            Time = request.Time ?? "00:00",
//    //            Duration = request.Duration > 0 ? request.Duration : 60,
//    //            Format = request.Format ?? "online",
//    //            Topic = request.Topic,
//    //            Comment = request.Comment,
//    //            Status = "pending",
//    //            CreatedAt = DateTime.UtcNow
//    //        };

//    //        _context.Meetings.Add(meeting);
//    //        await _context.SaveChangesAsync();

//    //        Console.WriteLine($"[CreateMeeting] Успешно создана встреча {meeting.Id}");

//    //        return Ok(new
//    //        {
//    //            message = "Встреча успешно создана",
//    //            meetingId = meeting.Id
//    //        });
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        Console.WriteLine($"[CreateMeeting] ОШИБКА: {ex.Message}");
//    //        Console.WriteLine($"[CreateMeeting] StackTrace: {ex.StackTrace}");

//    //        return StatusCode(500, new
//    //        {
//    //            message = "Внутренняя ошибка сервера при создании встречи",
//    //            error = ex.Message
//    //        });
//    //    }
//    //}

//    //// POST /api/meetings
//    //[HttpPost]
//    //public async Task<ActionResult> Create([FromBody] CreateMeetingRequest request)
//    //{
//    //    var userId = JwtMiddleware.GetUserId(HttpContext);
//    //    var userName = JwtMiddleware.GetUserName(HttpContext);

//    //    if (userId == Guid.Empty)
//    //        return Unauthorized(new { message = "Токен не предоставлен" });

//    //    if (userId == request.ReceiverId)
//    //        return BadRequest(new { message = "Нельзя создать встречу с самим собой" });

//    //    var meeting = new Meeting
//    //    {
//    //        Id = Guid.NewGuid(),
//    //        RequesterId = userId,
//    //        RequesterName = userName,
//    //        ReceiverId = request.ReceiverId,
//    //        ReceiverName = "User", // можно запросить из UserService
//    //        SkillId = request.SkillId,
//    //        SkillName = request.SkillName,
//    //        Date = request.Date,
//    //        Time = request.Time,
//    //        Duration = request.Duration,
//    //        Format = request.Format,
//    //        Topic = request.Topic,
//    //        Comment = request.Comment,
//    //        Status = "pending",
//    //        CreatedAt = DateTime.UtcNow
//    //    };

//    //    _context.Meetings.Add(meeting);
//    //    await _context.SaveChangesAsync();

//    //    return CreatedAtAction(nameof(GetById), new { id = meeting.Id }, meeting);
//    //}

//    // GET /api/meetings/{id}
//    [HttpGet("{id:guid}")]
//    public async Task<ActionResult> GetById(Guid id)
//    {
//        var meeting = await _context.Meetings.FindAsync(id);
//        if (meeting is null)
//            return NotFound(new { message = "Встреча не найдена" });

//        return Ok(meeting);
//    }

//    // PATCH /api/meetings/{id}/status
//    [HttpPatch("{id:guid}/status")]
//    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
//    {
//        var userId = JwtMiddleware.GetUserId(HttpContext);
//        if (userId == Guid.Empty)
//            return Unauthorized(new { message = "Токен не предоставлен" });

//        var meeting = await _context.Meetings.FindAsync(id);
//        if (meeting is null)
//            return NotFound(new { message = "Встреча не найдена" });

//        if (meeting.ReceiverId != userId && meeting.RequesterId != userId)
//            return StatusCode(403, new { message = "Нет доступа" });

//        meeting.Status = request.Status;
//        await _context.SaveChangesAsync();

//        return Ok(meeting);
//    }

//    // PATCH /api/meetings/update-past — авто-завершение прошедших
//    [HttpPatch("update-past")]
//    public async Task<ActionResult> UpdatePast()
//    {
//        var today = DateTime.UtcNow.Date;
//        var pastMeetings = await _context.Meetings
//            .Where(m => m.Date < today && m.Status != "completed" && m.Status != "cancelled")
//            .ToListAsync();

//        foreach (var m in pastMeetings)
//            m.Status = "completed";

//        await _context.SaveChangesAsync();

//        return Ok(new { updated = pastMeetings.Count });
//    }
//}