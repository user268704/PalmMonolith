namespace Palm.Models.Sessions;

public class Take
{
    public Guid Id { get; set; }
    public string StudentId { get; set; }
    public string ConnectionId { get; set; }
    public List<QuestionAnswer> QuestionAnswers { get; set; }
    public TimeOnly TimeStart { get; set; }
    public TimeOnly TimeCompleted { get; set; }
}

public class TakeComparer : IEqualityComparer<Take>
{
    public bool Equals(Take x, Take y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;

        return x.StudentId == y.StudentId &&
               x.TimeStart.Equals(y.TimeStart) &&
               x.TimeCompleted.Equals(y.TimeCompleted);
    }

    public int GetHashCode(Take obj)
    {
        return HashCode.Combine(obj.StudentId, obj.QuestionAnswers, obj.TimeStart, obj.TimeCompleted);
    }
}
