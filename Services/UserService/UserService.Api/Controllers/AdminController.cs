using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Api.Data;
using UserService.Api.DTOs;
using UserService.Api.Models;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly UserDbContext _context;

    public AdminController(UserDbContext context)
    {
        _context = context;
    }

    // Получить список пользователей
    [HttpGet("users")]
    public async Task<ActionResult> GetUsers([FromQuery] int page = 1,
                                             [FromQuery] int pageSize = 20,
                                             [FromQuery] string? search = null)
    {
        var query = _context.Users
            .Include(u => u.BannedUser)
            .Include(u => u.UserSkills)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Name.Contains(search) ||
                                   u.Email.Contains(search));
        }

        var total = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserAdminDto
            {
                Id = u.Id,
                FullName = u.Name,
                Email = u.Email,
                AvatarUrl = u.Avatar,
                Bio = u.Bio ?? "",
                IsBanned = u.BannedUser != null && u.BannedUser.IsActive,
                BannedUntil = u.BannedUser != null ? u.BannedUser.BannedUntil : null,
                SkillsCount = u.UserSkills.Count,
                ReviewsCount = 0,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(new { users, total, page, pageSize });
    }

    // Детальная информация
    [HttpGet("users/{id:guid}")]
    public async Task<ActionResult> GetUserDetail(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.UserSkills)
            .Include(u => u.BannedUser)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        return Ok(user);
    }

    // Забанить
    [HttpPost("users/{id:guid}/ban")]
    public async Task<ActionResult> BanUser(Guid id, [FromBody] BanRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        // Снимаем старый активный бан
        var existingBan = await _context.BannedUsers
            .FirstOrDefaultAsync(b => b.UserId == id && b.IsActive);

        if (existingBan != null)
            existingBan.IsActive = false;

        var ban = new BannedUser
        {
            UserId = id,
            Email = user.Email,
            Reason = request.Reason ?? "Без указания причины",
            BannedUntil = request.Days.HasValue ? DateTime.UtcNow.AddDays(request.Days.Value) : null,
            BannedByAdminId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
        };

        _context.BannedUsers.Add(ban);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Пользователь успешно забанен" });
    }

    // Разбанить
    [HttpPost("users/{id:guid}/unban")]
    public async Task<ActionResult> UnbanUser(Guid id)
    {
        var ban = await _context.BannedUsers
            .FirstOrDefaultAsync(b => b.UserId == id && b.IsActive);

        if (ban != null)
        {
            ban.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Пользователь разбанен" });
        }

        return BadRequest(new { message = "Активный бан не найден" });
    }
}


















//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using UserService.Api.Data;
//using UserService.Api.DTOs;           // если у тебя BanRequest и UserAdminDto здесь
//using UserService.Api.Models;

//namespace UserService.Api.Controllers;

//[ApiController]
//[Route("api/admin")]
//[Authorize(Roles = "Admin")]
//public class AdminController : ControllerBase
//{
//    private readonly UserDbContext _context;

//    public AdminController(UserDbContext context)
//    {
//        _context = context;
//    }


//    // Получить список всех пользователей
//    [HttpGet("users")]
//    public async Task<ActionResult> GetUsers([FromQuery] int page = 1,
//                                             [FromQuery] int pageSize = 20,
//                                             [FromQuery] string? search = null)
//    {
//        var query = _context.Users
//            .Include(u => u.BannedUser)
//            .Include(u => u.UserSkills)
//            .AsQueryable();

//        if (!string.IsNullOrEmpty(search))
//        {
//            query = query.Where(u => u.Name.Contains(search) ||
//                                   u.Email.Contains(search));
//        }

//        var total = await query.CountAsync();

//        var users = await query
//            .OrderByDescending(u => u.CreatedAt)
//            .Skip((page - 1) * pageSize)
//            .Take(pageSize)
//            .Select(u => new UserAdminDto
//            {
//                Id = u.Id,
//                FullName = u.Name,
//                Email = u.Email,
//                AvatarUrl = u.Avatar,
//                Bio = u.Bio ?? "",
//                IsBanned = u.BannedUser != null && u.BannedUser.IsActive,
//                BannedUntil = u.BannedUser != null ? u.BannedUser.BannedUntil : null,   // ← исправлено
//                SkillsCount = u.UserSkills.Count,
//                ReviewsCount = 0,
//                CreatedAt = u.CreatedAt
//            })
//            .ToListAsync();

//        return Ok(new { users, total, page, pageSize });
//    }

//    // Детальная информация о пользователе
//    [HttpGet("users/{id:guid}")]
//    public async Task<ActionResult> GetUserDetail(Guid id)
//    {
//        var user = await _context.Users
//            .Include(u => u.UserSkills)
//            .Include(u => u.BannedUser)
//            .FirstOrDefaultAsync(u => u.Id == id);

//        if (user == null)
//            return NotFound(new { message = "Пользователь не найден" });

//        return Ok(user);
//    }

//    // Бан пользователя
//    [HttpPost("users/{id:guid}/ban")]
//    public async Task<ActionResult> BanUser(Guid id, [FromBody] BanRequest request)
//    {
//        var user = await _context.Users.FindAsync(id);
//        if (user == null)
//            return NotFound(new { message = "Пользователь не найден" });

//        // Снимаем предыдущий активный бан
//        var existingBan = await _context.BannedUsers
//            .FirstOrDefaultAsync(b => b.UserId == id && b.IsActive);

//        if (existingBan != null)
//        {
//            existingBan.IsActive = false;
//        }

//        var ban = new BannedUser
//        {
//            UserId = id,
//            Email = user.Email,
//            Reason = request.Reason ?? "Без указания причины",
//            BannedUntil = request.Days.HasValue
//                          ? DateTime.UtcNow.AddDays(request.Days.Value)
//                          : null,
//            BannedByAdminId = User.FindFirst("sub")?.Value,   // ID текущего админа
//        };

//        _context.BannedUsers.Add(ban);
//        await _context.SaveChangesAsync();

//        return Ok(new { message = "Пользователь успешно забанен" });
//    }

//    // Разбан пользователя
//    [HttpPost("users/{id:guid}/unban")]
//    public async Task<ActionResult> UnbanUser(Guid id)
//    {
//        var ban = await _context.BannedUsers
//            .FirstOrDefaultAsync(b => b.UserId == id && b.IsActive);

//        if (ban != null)
//        {
//            ban.IsActive = false;
//            await _context.SaveChangesAsync();
//            return Ok(new { message = "Пользователь разбанен" });
//        }

//        return BadRequest(new { message = "Активный бан не найден" });
//    }
//}














//namespace UserService.Api.Controllers;


//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using UserService.Api.Data;
//using UserService.Api.DTOs;
//using UserService.Api.Models;

//[ApiController]
//[Route("api/admin")]
//[Authorize(Roles = "Admin")]
//public class AdminController : ControllerBase
//{
//    private readonly UserDbContext _context;

//    public AdminController(UserDbContext context)
//    {
//        _context = context;
//    }

//    // Получить список всех пользователей
//    [HttpGet("users")]
//    public async Task<ActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
//                                             [FromQuery] string? search = null)
//    {
//        var query = _context.Users.Include(u => u.BannedUser).AsQueryable();

//        if (!string.IsNullOrEmpty(search))
//        {
//            query = query.Where(u => u.FullName.Contains(search) ||
//                                   u.Email.Contains(search));
//        }

//        var total = await query.CountAsync();
//        var users = await query
//            .OrderByDescending(u => u.CreatedAt)
//            .Skip((page - 1) * pageSize)
//            .Take(pageSize)
//            .Select(u => new UserAdminDto
//            {
//                Id = u.Id,
//                FullName = u.FullName,
//                Email = u.Email,
//                AvatarUrl = u.AvatarUrl,
//                Bio = u.Bio ?? "",
//                IsBanned = u.BannedUser != null && u.BannedUser.IsActive,
//                BannedUntil = u.BannedUser?.BannedUntil,
//                SkillsCount = u.UserSkills.Count,
//                ReviewsCount = u.ReceivedReviews.Count,
//                CreatedAt = u.CreatedAt
//            })
//            .ToListAsync();

//        return Ok(new { users, total, page, pageSize });
//    }

//    // Детальная информация о пользователе
//    [HttpGet("users/{id:guid}")]
//    public async Task<ActionResult> GetUserDetail(Guid id)
//    {
//        var user = await _context.Users
//            .Include(u => u.UserSkills)
//            .Include(u => u.ReceivedReviews)
//            .Include(u => u.BannedUser)
//            .FirstOrDefaultAsync(u => u.Id == id);

//        if (user == null) return NotFound();

//        return Ok(user);
//    }

//    // Бан пользователя
//    [HttpPost("users/{id:guid}/ban")]
//    public async Task<ActionResult> BanUser(Guid id, [FromBody] BanRequest request)
//    {
//        var user = await _context.Users.FindAsync(id);
//        if (user == null) return NotFound();

//        var existingBan = await _context.BannedUsers
//            .FirstOrDefaultAsync(b => b.UserId == id && b.IsActive);

//        if (existingBan != null)
//        {
//            existingBan.IsActive = false; // снимаем старый бан
//        }

//        var ban = new BannedUser
//        {
//            UserId = id,
//            Email = user.Email,
//            Reason = request.Reason,
//            BannedUntil = request.Days.HasValue ? DateTime.UtcNow.AddDays(request.Days.Value) : null
//        };

//        _context.BannedUsers.Add(ban);
//        await _context.SaveChangesAsync();

//        return Ok(new { message = "Пользователь забанен" });
//    }

//    // Разбан пользователя
//    [HttpPost("users/{id:guid}/unban")]
//    public async Task<ActionResult> UnbanUser(Guid id)
//    {
//        var ban = await _context.BannedUsers
//            .FirstOrDefaultAsync(b => b.UserId == id && b.IsActive);

//        if (ban != null)
//        {
//            ban.IsActive = false;
//            await _context.SaveChangesAsync();
//        }

//        return Ok(new { message = "Пользователь разбанен" });
//    }
//}
