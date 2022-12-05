namespace Palm.Models.Sessions.Dto;

public class StudentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
}