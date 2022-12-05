using System.Security.Cryptography;

namespace Palm.Models.Errors;

public class SessionCreateError
{
    public string Message { get; set; }
    public string Trace { get; set; }
    public string Type { get; set; }
    public Guid? SessionId { get; set; }
}