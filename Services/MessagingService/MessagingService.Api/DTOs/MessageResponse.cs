namespace MessagingService.Api.DTOs;

public class MessageResponse
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Guid ReceiverId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public class SendMessageRequest
{
    public Guid ReceiverId { get; set; }
    public string Text { get; set; } = string.Empty;
}