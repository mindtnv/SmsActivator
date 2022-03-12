namespace SmsActivator.Abstractions;

public class ActivatorException : Exception
{
    public string Message { get; set; }

    public ActivatorException(string message)
    {
        Message = message;
    }
}