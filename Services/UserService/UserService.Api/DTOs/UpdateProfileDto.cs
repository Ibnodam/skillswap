namespace UserService.Api.DTOs;

public class UpdateProfileDto
{
    public string City { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Bio { get; set; }

    public List<string> OfferSkills { get; set; } = new();
    public List<string> SeekSkills { get; set; } = new();
}
