using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Api.Data;
using UserService.Api.DTOs;
using UserService.Api.Models;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _context;

    public UsersController(UserDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var users = await _context.Users
            .Include(u => u.UserSkills)
            .ThenInclude(us => us.Skill)
            .OrderBy(u => u.Id)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Avatar = u.Avatar,
                City = u.City,
                Skills = new SkillsDto
                {
                    Offer = u.UserSkills
                        .Where(us => us.Type == "offer")
                        .Select(us => us.Skill.Name)
                        .ToList(),
                    Seek = u.UserSkills
                        .Where(us => us.Type == "seek")
                        .Select(us => us.Skill.Name)
                        .ToList()
                }
            })
            .ToListAsync();

        var total = await _context.Users.CountAsync();

        return Ok(new
        {
            users,
            pagination = new
            {
                page,
                limit,
                total,
                pages = (int)Math.Ceiling(total / (double)limit)
            }
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.UserSkills)
            .ThenInclude(us => us.Skill)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return NotFound(new { message = "Пользователь не найден" });

        return Ok(new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Avatar = user.Avatar,
            City = user.City,
            Bio = user.Bio,
            IsPremium = user.IsPremium,
            CreatedAt = user.CreatedAt,
            Skills = new SkillsDto
            {
                Offer = user.UserSkills
                    .Where(us => us.Type == "offer")
                    .Select(us => us.Skill.Name)
                    .ToList(),
                Seek = user.UserSkills
                    .Where(us => us.Type == "seek")
                    .Select(us => us.Skill.Name)
                    .ToList()
            }
        });
    }

    // POST /api/users/internal — для вызова из других сервисов
    [HttpPost("internal")]
    public async Task<ActionResult> CreateInternal([FromBody] CreateUserInternalRequest request)
    {
        var exists = await _context.Users.AnyAsync(u => u.Id == request.Id);
        if (exists)
            return Ok(new { message = "Пользователь уже существует" });

        var user = new User
        {
            Id = request.Id,
            Email = request.Email,
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Пользователь создан" });
    }
}