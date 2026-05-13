using System.Text.Json.Serialization;

namespace UserService.Api.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? City { get; set; }
    public string? Bio { get; set; }
    public bool IsPremium { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsBanned { get; set; } = false;
    public DateTime? BanUntil { get; set; }

    public bool IsActive { get; set; } = true;

    // === Добавь эти два свойства ===
    public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();


    public virtual BannedUser? BannedUser { get; set; }   // ← важно


}