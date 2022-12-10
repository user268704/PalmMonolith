namespace Palm.Models.Sessions;

public class QuestionAnswer
{
    public Guid Id { get; set; }
    public string QuestionId { get; set; }
    public int AnswerId { get; set; }
}