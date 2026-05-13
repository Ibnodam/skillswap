using UserService.Api.Models;

namespace UsersService.Api.Models;

public class UserBan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;   // навигационное свойство

    public Guid BannedBy { get; set; }                // Id админа
    public string? BanReason { get; set; }

    public DateTime BannedAt { get; set; } = DateTime.UtcNow;
    public DateTime? BanUntil { get; set; }           // null = бан навсегда

    public bool IsActive { get; set; } = true;
}