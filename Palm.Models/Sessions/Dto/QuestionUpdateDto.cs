namespace Palm.Models.Sessions.Dto;

public class QuestionUpdateDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public IEnumerable<AnswerUpdateDto> Answers { get; set; }
}