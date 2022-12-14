namespace Palm.Models.Sessions.Dto;

public class SessionStudentDto
{
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public List<QuestionUpdateDto> Questions { get; set; }
}