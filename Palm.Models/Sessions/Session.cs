namespace Palm.Models.Sessions;

public class Session
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    public string ShortId { get; set; }
    public string Title { get; set; }
    public SessionGroupInfo GroupInfo { get; set; }
    public List<string> Students { get; set; }
    public List<string> Questions { get; set; }
    public List<Take> Takes { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}