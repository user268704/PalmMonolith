namespace Palm.Models.Sessions.Dto;

public class SessionDto
{
    public string Title { get; set; }
    public List<QuestionUpdateDto>? Questions { get; set; }
    public Guid? ClassId { get; set; }
    public bool IsConnectAfterStart { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}