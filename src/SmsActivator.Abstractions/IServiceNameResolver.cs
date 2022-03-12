namespace SmsActivator.Abstractions;

public interface IServiceNameResolver
{
    string ResolveServiceName(string service);
}