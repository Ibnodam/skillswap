namespace UserService.Api.DTOs;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? City { get; set; }
    public string? Bio { get; set; }
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }
    public bool IsPremium { get; set; }
    public SkillsDto Skills { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class SkillsDto
{
    public List<string> Offer { get; set; } = new();
    public List<string> Seek { get; set; } = new();
}