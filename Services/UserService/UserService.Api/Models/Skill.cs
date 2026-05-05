using System.Text.Json.Serialization;

namespace UserService.Api.Models;

public class Skill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }

    [JsonIgnore]
    public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
}