using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Api.Data;
using UserService.Api.DTOs;
using UserService.Api.Models;
using System.Security.Claims;

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
                Address = u.Address,
                Rating = 0, 
                ReviewsCount = 0, 
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
            pagination = new { page, limit, total, pages = (int)Math.Ceiling(total / (double)limit) }
        });
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {

        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"{claim.Type} = {claim.Value}");
        }

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("UserId")?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) ||
            !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new
            {
                message = "Не удалось определить пользователя"
            });
        }

        var user = await _context.Users
            .Include(u => u.UserSkills)
            .ThenInclude(us => us.Skill)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "Пользователь не найден"
            });
        }

        user.City = dto.City?.Trim();
        user.Address = dto.Address?.Trim();
        user.Bio = dto.Bio?.Trim();

        var currentOffer = user.UserSkills
            .Where(us => us.Type == "offer")
            .ToList();

        var currentSeek = user.UserSkills
            .Where(us => us.Type == "seek")
            .ToList();

        // ===== НОВЫЕ НАВЫКИ =====
        var newOfferNames = dto.OfferSkills
            .Select(s => s.Trim().ToLowerInvariant())
            .ToHashSet();

        var newSeekNames = dto.SeekSkills
            .Select(s => s.Trim().ToLowerInvariant())
            .ToHashSet();

        // ===== УДАЛЯЕМ УБРАННЫЕ НАВЫКИ =====
        var toRemove = currentOffer
            .Where(us => !newOfferNames.Contains(us.Skill.Name.ToLowerInvariant()))
            .ToList();

        toRemove.AddRange(
            currentSeek.Where(us =>
                !newSeekNames.Contains(us.Skill.Name.ToLowerInvariant()))
        );

        if (toRemove.Any())
        {
            _context.UserSkills.RemoveRange(toRemove);
        }

        // ===== ДОБАВЛЯЕМ OFFER SKILLS =====
        foreach (var skillName in dto.OfferSkills)
        {
            var trimmedName = skillName.Trim();

            var alreadyExists = currentOffer.Any(us =>
                us.Skill.Name.Equals(trimmedName,
                    StringComparison.OrdinalIgnoreCase));

            if (!alreadyExists)
            {
                var skill = await GetOrCreateSkillAsync(trimmedName);

                _context.UserSkills.Add(new UserSkill
                {
                    UserId = user.Id,
                    SkillId = skill.Id,
                    Type = "offer"
                });
            }
        }

        // ===== ДОБАВЛЯЕМ SEEK SKILLS =====
        foreach (var skillName in dto.SeekSkills)
        {
            var trimmedName = skillName.Trim();

            var alreadyExists = currentSeek.Any(us =>
                us.Skill.Name.Equals(trimmedName,
                    StringComparison.OrdinalIgnoreCase));

            if (!alreadyExists)
            {
                var skill = await GetOrCreateSkillAsync(trimmedName);

                _context.UserSkills.Add(new UserSkill
                {
                    UserId = user.Id,
                    SkillId = skill.Id,
                    Type = "seek"
                });
            }
        }

        // ===== СОХРАНЯЕМ =====
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Профиль успешно обновлён"
        });
    }

    private async Task<Skill> GetOrCreateSkillAsync(string name)
    {
        var normalized = name.Trim();

        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Name.ToLower() == normalized.ToLower());

        if (skill != null)
            return skill;

        skill = new Skill
        {
            Name = normalized,
            Category = null
        };

        _context.Skills.Add(skill);

        return skill;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult> GetCurrentUser()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("UserId")?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) ||
            !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new { message = "Не удалось определить пользователя" });
        }

        var user = await _context.Users
            .Include(u => u.UserSkills)
            .ThenInclude(us => us.Skill)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        return Ok(new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Avatar = user.Avatar,
            City = user.City,
            Address = user.Address,
            Bio = user.Bio,
            Rating = 0,        // ← Добавить
            ReviewsCount = 0,  // ← Добавить
            CreatedAt = user.CreatedAt,
            Skills = new SkillsDto
            {
                Offer = user.UserSkills
                    .Where(us => us.Type == "offer" && us.Skill != null)
                    .Select(us => us.Skill.Name)
                    .ToList(),
                Seek = user.UserSkills
                    .Where(us => us.Type == "seek" && us.Skill != null)
                    .Select(us => us.Skill.Name)
                    .ToList()
            }
        });
    }

    [HttpGet("skills")]
    public async Task<ActionResult> GetAllSkills([FromQuery] string? search)
    {
        var query = _context.Skills.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Name.ToLower().Contains(search.ToLower()));
        }

        var skills = await query
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name, s.Category })
            .ToListAsync();

        return Ok(skills);
    }



    //[HttpGet("{id:guid}")]
    //public async Task<ActionResult> GetById(Guid id)
    //{
    //    var user = await _context.Users
    //        .Include(u => u.UserSkills)
    //        .ThenInclude(us => us.Skill)
    //        .FirstOrDefaultAsync(u => u.Id == id);

    //    if (user == null)
    //        return NotFound(new { message = "Пользователь не найден" });

    //    return Ok(new UserResponse
    //    {
    //        Id = user.Id,
    //        Name = user.Name,
    //        Avatar = user.Avatar,
    //        City = user.City,
    //        Address = user.Address,
    //        Bio = user.Bio,
    //        CreatedAt = user.CreatedAt,
    //        Skills = new SkillsDto
    //        {
    //            Offer = user.UserSkills
    //                .Where(us => us.Type == "offer" && us.Skill != null)
    //                .Select(us => us.Skill.Name)
    //                .ToList(),
    //            Seek = user.UserSkills
    //                .Where(us => us.Type == "seek" && us.Skill != null)
    //                .Select(us => us.Skill.Name)
    //                .ToList()
    //        }
    //    });
    //}


    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.UserSkills)
            .ThenInclude(us => us.Skill)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        return Ok(new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Avatar = user.Avatar,
            City = user.City,
            Address = user.Address,
            Bio = user.Bio,
            Rating = 0, // ← Временно 0
            ReviewsCount = 0, // ← Временно 0
            CreatedAt = user.CreatedAt,
            Skills = new SkillsDto
            {
                Offer = user.UserSkills
                    .Where(us => us.Type == "offer" && us.Skill != null)
                    .Select(us => us.Skill.Name)
                    .ToList(),
                Seek = user.UserSkills
                    .Where(us => us.Type == "seek" && us.Skill != null)
                    .Select(us => us.Skill.Name)
                    .ToList()
            }
        });
    }

    //[HttpGet("{id:guid}")]
    //public async Task<ActionResult> GetById(Guid id)
    //{
    //    var user = await _context.Users
    //        .Include(u => u.UserSkills)
    //        .ThenInclude(us => us.Skill)
    //        .FirstOrDefaultAsync(u => u.Id == id);

    //    if (user is null)
    //        return NotFound(new { message = "Пользователь не найден" });

    //    return Ok(new UserResponse
    //    {
    //        Id = user.Id,
    //        Name = user.Name,
    //        Avatar = user.Avatar,
    //        City = user.City,
    //        Bio = user.Bio,
    //        //IsPremium = user.IsPremium,
    //        CreatedAt = user.CreatedAt,
    //        Skills = new SkillsDto
    //        {
    //            Offer = user.UserSkills
    //                .Where(us => us.Type == "offer")
    //                .Select(us => us.Skill.Name)
    //                .ToList(),
    //            Seek = user.UserSkills
    //                .Where(us => us.Type == "seek")
    //                .Select(us => us.Skill.Name)
    //                .ToList()
    //        }
    //    });
    //}


    [HttpGet("{userId:guid}/skills/{type}")]
    public async Task<ActionResult> GetUserSkills(Guid userId, string type)
    {
        var skills = await _context.UserSkills
            .Include(us => us.Skill)
            .Where(us => us.UserId == userId && us.Type == type)
            .Select(us => new { us.Skill.Id, us.Skill.Name })
            .ToListAsync();

        return Ok(skills);
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





//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using UserService.Api.Data;
//using UserService.Api.DTOs;
//using UserService.Api.Models;

//namespace UserService.Api.Controllers;

//[ApiController]
//[Route("api/users")]
//public class UsersController : ControllerBase
//{
//    private readonly UserDbContext _context;

//    public UsersController(UserDbContext context)
//    {
//        _context = context;
//    }

//    [HttpGet]
//    public async Task<ActionResult> GetAll(
//        [FromQuery] int page = 1,
//        [FromQuery] int limit = 10)
//    {
//        var users = await _context.Users
//            .Include(u => u.UserSkills)
//            .ThenInclude(us => us.Skill)
//            .OrderBy(u => u.Id)
//            .Skip((page - 1) * limit)
//            .Take(limit)
//            .ToListAsync();

//        var result = users.Select(u => new UserResponse
//        {
//            Id = u.Id,
//            Name = u.Name,
//            Avatar = u.Avatar,
//            City = u.City,
//            CreatedAt = u.CreatedAt.ToString("yyyy-MM-dd"),
//            Skills = new SkillsDto
//            {
//                Offer = u.UserSkills
//                    .Where(us => us.Type == "offer")
//                    .Select(us => us.Skill.Name)
//                    .ToList(),
//                Seek = u.UserSkills
//                    .Where(us => us.Type == "seek")
//                    .Select(us => us.Skill.Name)
//                    .ToList()
//            }
//        }).ToList();

//        var total = await _context.Users.CountAsync();

//        return Ok(new
//        {
//            users = result,
//            pagination = new
//            {
//                page,
//                limit,
//                total,
//                pages = (int)Math.Ceiling(total / (double)limit)
//            }
//        });
//    }

//    [HttpGet("{id:guid}")]
//    public async Task<ActionResult> GetById(Guid id)
//    {
//        var user = await _context.Users
//            .Include(u => u.UserSkills)
//            .ThenInclude(us => us.Skill)
//            .FirstOrDefaultAsync(u => u.Id == id);

//        if (user is null)
//            return NotFound(new { message = "Пользователь не найден" });

//        return Ok(new UserResponse
//        {
//            Id = user.Id,
//            Name = user.Name,
//            Avatar = user.Avatar,
//            City = user.City,
//            Bio = user.Bio,
//            IsPremium = user.IsPremium,
//            CreatedAt = user.CreatedAt.ToString("yyyy-MM-dd"),
//            Skills = new SkillsDto
//            {
//                Offer = user.UserSkills
//                    .Where(us => us.Type == "offer")
//                    .Select(us => us.Skill.Name)
//                    .ToList(),
//                Seek = user.UserSkills
//                    .Where(us => us.Type == "seek")
//                    .Select(us => us.Skill.Name)
//                    .ToList()
//            }
//        });
//    }

//    [HttpPost("internal")]
//    public async Task<ActionResult> CreateInternal([FromBody] CreateUserInternalRequest request)
//    {
//        var exists = await _context.Users.AnyAsync(u => u.Id == request.Id);
//        if (exists)
//            return Ok(new { message = "Пользователь уже существует" });

//        var user = new User
//        {
//            Id = request.Id,
//            Email = request.Email,
//            Name = request.Name,
//            CreatedAt = DateTime.UtcNow
//        };

//        _context.Users.Add(user);
//        await _context.SaveChangesAsync();

//        return Ok(new { message = "Пользователь создан" });
//    }
//}


