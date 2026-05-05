using System.Text.Json.Serialization;

namespace UserService.Api.Models;

public class UserSkill
{
    public Guid UserId { get; set; }

    [JsonIgnore]
    public User User { get; set; } = null!;

    public Guid SkillId { get; set; }

    [JsonIgnore]
    public Skill Skill { get; set; } = null!;

    public string Type { get; set; } = "offer"; // offer или seek
}