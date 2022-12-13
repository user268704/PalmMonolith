namespace Palm.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message)
    {
        Message = message;
    }

    public NotFoundException(string message, string whatNotHound)
    {
        Message = message;
        WhatNotHound = whatNotHound;
    }

    public NotFoundException()
    {
        
    }

    public new string Message { get; private set; }
    public string WhatNotHound { get; private set; }
}