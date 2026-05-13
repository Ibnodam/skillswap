namespace MessagingService.Api.Models;

public class Message
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;

    public Guid ReceiverId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;   // ← Добавь это

    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}










//namespace MessagingService.Api.Models;

//public class Message
//{
//    public Guid Id { get; set; }
//    public Guid SenderId { get; set; }
//    public string SenderName { get; set; } = string.Empty;
//    public Guid ReceiverId { get; set; }
//    public string Text { get; set; } = string.Empty;
//    public DateTime SentAt { get; set; } = DateTime.UtcNow;
//}