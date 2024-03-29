﻿namespace Palm.Models.Sessions;

public class Question
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public IEnumerable<Answer> Answers { get; set; }
}