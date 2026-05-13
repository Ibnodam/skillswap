namespace UserService.Api.Models;


public class BannedUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime BannedAt { get; set; } = DateTime.UtcNow;
    public DateTime? BannedUntil { get; set; }
    public bool IsActive { get; set; } = true;

    public string? BannedByAdminId { get; set; }
    public string? BannedByAdminEmail { get; set; }

    public virtual User User { get; set; } = null!;
}