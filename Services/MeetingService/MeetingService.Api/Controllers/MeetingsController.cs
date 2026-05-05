using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeetingService.Api.Data;
using MeetingService.Api.DTOs;
using MeetingService.Api.Middleware;
using MeetingService.Api.Models;

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

    // GET /api/meetings/my
    [HttpGet("my")]
    public async Task<ActionResult> GetMyMeetings()
    {
        var userId = JwtMiddleware.GetUserId(HttpContext);
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "Токен не предоставлен" });

        var meetings = await _context.Meetings
            .Where(m => m.RequesterId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.Date)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;

        var upcoming = meetings.Where(m => m.Date >= today && m.Status != "cancelled" && m.Status != "completed").ToList();
        var past = meetings.Where(m => m.Date < today || m.Status == "completed" || m.Status == "cancelled").ToList();

        return Ok(new { upcoming, past });
    }

    // GET /api/meetings/calendar?year=2026&month=5
    [HttpGet("calendar")]
    public async Task<ActionResult> GetCalendar([FromQuery] int year, [FromQuery] int month)
    {
        var userId = JwtMiddleware.GetUserId(HttpContext);
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "Токен не предоставлен" });

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var startDate = new DateTime(year, month, 1);
        var endDate = new DateTime(year, month, daysInMonth);

        var meetings = await _context.Meetings
            .Where(m => (m.RequesterId == userId || m.ReceiverId == userId)
                        && m.Date >= startDate && m.Date <= endDate)
            .ToListAsync();

        var calendar = new Dictionary<int, List<MeetingResponse>>();
        foreach (var m in meetings)
        {
            var day = m.Date.Day;
            if (!calendar.ContainsKey(day))
                calendar[day] = new List<MeetingResponse>();

            calendar[day].Add(new MeetingResponse
            {
                Id = m.Id,
                RequesterId = m.RequesterId,
                RequesterName = m.RequesterName,
                ReceiverId = m.ReceiverId,
                ReceiverName = m.ReceiverName,
                SkillId = m.SkillId,
                SkillName = m.SkillName,
                Date = m.Date,
                Time = m.Time,
                Duration = m.Duration,
                Format = m.Format,
                Topic = m.Topic,
                Status = m.Status
            });
        }

        return Ok(calendar);
    }

    // POST /api/meetings
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateMeetingRequest request)
    {
        var userId = JwtMiddleware.GetUserId(HttpContext);
        var userName = JwtMiddleware.GetUserName(HttpContext);

        if (userId == Guid.Empty)
            return Unauthorized(new { message = "Токен не предоставлен" });

        if (userId == request.ReceiverId)
            return BadRequest(new { message = "Нельзя создать встречу с самим собой" });

        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            RequesterId = userId,
            RequesterName = userName,
            ReceiverId = request.ReceiverId,
            ReceiverName = "User", // можно запросить из UserService
            SkillId = request.SkillId,
            SkillName = request.SkillName,
            Date = request.Date,
            Time = request.Time,
            Duration = request.Duration,
            Format = request.Format,
            Topic = request.Topic,
            Comment = request.Comment,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = meeting.Id }, meeting);
    }

    // GET /api/meetings/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var meeting = await _context.Meetings.FindAsync(id);
        if (meeting is null)
            return NotFound(new { message = "Встреча не найдена" });

        return Ok(meeting);
    }

    // PATCH /api/meetings/{id}/status
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

    // PATCH /api/meetings/update-past — авто-завершение прошедших
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