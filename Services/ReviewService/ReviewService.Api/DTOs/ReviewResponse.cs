namespace ReviewService.Api.DTOs;

public class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid FromUserId { get; set; }
    public string FromUserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewRequest
{
    public Guid ToUserId { get; set; }
    public string ToUserName { get; set; } = string.Empty;
    public Guid? MeetingId { get; set; }
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
}