using Palm.Models.Sessions;

namespace Palm.Abstractions.Interfaces.Data;

public interface IQuestionsCaching
{
    public Question GetQuestion(string sessionId, string questionId);
    public List<Question> GetQuestionsFromSession(string sessionId);
    public void AddQuestion(Question question, string sessionId);
    public void AddQuestion(Question question, Session session);

    public List<string> AddQuestions(List<Question> questions, string sessionId);
    public void CreateQuestion(string sessionId);
}