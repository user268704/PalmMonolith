namespace Palm.Models.Sessions.Dto;

public class SessionUpdateDto
{
    public string? Title { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string>? Students { get; set; }
    public ICollection<QuestionUpdateDto>? Questions { get; set; }
}