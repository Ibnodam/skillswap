namespace UserService.Api.DTOs;

public class BanRequest
{
    public string Reason { get; set; } = string.Empty;
    public int? Days { get; set; }        
}

public class UserAdminDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Bio { get; set; } = string.Empty;
    public bool IsBanned { get; set; }
    public DateTime? BannedUntil { get; set; }
    public int SkillsCount { get; set; }
    public int ReviewsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}