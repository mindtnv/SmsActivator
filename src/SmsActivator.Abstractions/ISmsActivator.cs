namespace SmsActivator.Abstractions;

public interface ISmsActivator
{
    Task<IActivationManager> ActivateAsync(ActivationParameters parameters);
}