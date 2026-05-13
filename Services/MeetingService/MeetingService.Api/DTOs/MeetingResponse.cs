namespace MeetingService.Api.DTOs;

public class MeetingResponse
{
    public Guid Id { get; set; }
    public Guid RequesterId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public Guid ReceiverId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Time { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string Format { get; set; } = string.Empty;
    public string? Topic { get; set; }
    public string? Comment { get; set; }
    public string Status { get; set; } = string.Empty;
}


public class CreateMeetingRequest
{
    public Guid ReceiverId { get; set; }
    public Guid SkillId { get; set; }           // ← оставляем Guid
    public string? SkillName { get; set; }


    public DateTime Date { get; set; }
    public string Time { get; set; } = string.Empty;
    //public DateOnly Date { get; set; }          // ← Лучше использовать DateOnly
    //public TimeOnly Time { get; set; }          // ← Лучше использовать TimeOnly (.NET 6+)

    public int Duration { get; set; } = 60;
    public string Format { get; set; } = "online";
    public string? Topic { get; set; }
    public string? Comment { get; set; }
}


//public class CreateMeetingRequest
//{
//    public Guid ReceiverId { get; set; }
//    public Guid SkillId { get; set; }
//    public string SkillName { get; set; } = string.Empty;
//    public DateTime Date { get; set; }
//    public string Time { get; set; } = string.Empty;
//    public int Duration { get; set; } = 60;
//    public string Format { get; set; } = "online";
//    public string? Topic { get; set; }
//    public string? Comment { get; set; }
//}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}



//namespace MeetingService.Api.DTOs;

//public class MeetingResponse
//{
//    public Guid Id { get; set; }
//    public Guid RequesterId { get; set; }
//    public string RequesterName { get; set; } = string.Empty;
//    public Guid ReceiverId { get; set; }
//    public string ReceiverName { get; set; } = string.Empty;
//    public Guid SkillId { get; set; }
//    public string SkillName { get; set; } = string.Empty;
//    public DateTime Date { get; set; }
//    public string Time { get; set; } = string.Empty;
//    public int Duration { get; set; }
//    public string Format { get; set; } = string.Empty;
//    public string? Topic { get; set; }
//    public string? Comment { get; set; }
//    public string Status { get; set; } = string.Empty;
//}

//public class CreateMeetingRequest
//{
//    public Guid ReceiverId { get; set; }
//    public string SkillName { get; set; } = string.Empty;
//    public DateTime Date { get; set; }
//    public string Time { get; set; } = string.Empty;
//    public int Duration { get; set; } = 60;
//    public string Format { get; set; } = "online";
//    public string? Topic { get; set; }
//    public string? Comment { get; set; }
//}

//public class UpdateStatusRequest
//{
//    public string Status { get; set; } = string.Empty;
//}
