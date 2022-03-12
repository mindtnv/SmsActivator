using SmsActivator.Abstractions;

namespace SmsActivator.SmsHub;

public class SmsHubActivatorException : ActivatorException
{
    public SmsHubActivatorException(string message) : base(message)
    {
    }
}