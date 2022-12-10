using System.Text.Json;
using Palm.Abstractions.Interfaces.Data;
using Palm.Models.Sessions;
using StackExchange.Redis;

namespace Palm.Data.Implementations;

public class QuestionsCaching : IQuestionsCaching
{
    private const string QUESTION_POSTFIX = "-questions";
    private readonly IDatabase _database;
    private readonly RedisConnect _redis;

    public QuestionsCaching()
    {
        _redis = RedisConnect.GetInstance();
        _database = _redis.GetDatabase();
    }

    public Question GetQuestion(string sessionId, string questionId)
    {
        string questionsJson =  _database.StringGet(sessionId + QUESTION_POSTFIX); 
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(questionsJson);

        return questions.Find(q => q.Id.ToString() == questionId);
    }

    public List<Question> GetQuestionsFromSession(string sessionId)
    {
        string questionsJson =  _database.StringGet(sessionId + QUESTION_POSTFIX); 
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(questionsJson);

        return questions;
    }

    public void AddQuestion(Question question, string sessionId)
    {
        string questionsJson =  _database.StringGet(sessionId + QUESTION_POSTFIX); 
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(questionsJson);
        
        questions.Add(question);
        questionsJson = JsonSerializer.Serialize(questions);
        _database.StringSet(sessionId + QUESTION_POSTFIX, questionsJson);
    }

    public void AddQuestion(Question question, Session session)
    {
        throw new NotImplementedException();
    }

    public List<string> AddQuestions(List<Question> questions, string sessionId)
    {
        string? questionsJson =  _database.StringGet(sessionId + QUESTION_POSTFIX);
        if (questionsJson == null)
            throw new ArgumentException("Session does not exist", nameof(sessionId));

        List<Question> questionsList = JsonSerializer.Deserialize<List<Question>>(questionsJson) 
                                       ?? new();

        foreach (Question question in questions)
        {
            if (question.Id == Guid.Empty) 
                question.Id = Guid.NewGuid();
            
            List<Answer> answers = question.Answers.ToList();
            for (var i = 0; i < answers.Count; i++)
            {
                var answer = answers[i];
                if (answer.Id == 0) 
                    answer.Id = i;
            }
        }
        
        questionsList.AddRange(questions);
        questionsJson = JsonSerializer.Serialize(questionsList);
        _database.StringSet(sessionId + QUESTION_POSTFIX, questionsJson);
        
        return questions.Select(x => x.Id.ToString()).ToList();
    }

    public void CreateQuestion(string sessionId) => 
        _database.StringSet(sessionId + QUESTION_POSTFIX, "[]");
}