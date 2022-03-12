namespace SmsActivator.Abstractions;

public interface IActivationManager : IAsyncDisposable
{
    public string Phone { get; }
    public string Id { get; }
    public ActivationParameters ActivationParameters { get; }
    public Task<string> GetCodeAsync(CancellationToken cancellationToken);
}