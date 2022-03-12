using SmsActivator.Abstractions;

namespace SmsActivator.SmsHub;

public class SmsHubActivationManager : IActivationManager
{
    private static readonly HttpClient Client = new();
    private readonly SmsHubSmsActivatorOptions _options;
    private bool _disposed;
    public bool CodeReceived { get; private set; }

    public SmsHubActivationManager(string phone, string id, ActivationParameters activationParameters,
        SmsHubSmsActivatorOptions options)
    {
        Phone = phone;
        Id = id;
        ActivationParameters = activationParameters;
        _options = options;
    }

    public string Phone { get; }
    public string Id { get; }
    public ActivationParameters ActivationParameters { get; }

    public async Task<string> GetCodeAsync(CancellationToken cancellationToken)
    {
        //https://smshub.org/stubs/handler_api.php?api_key=APIKEY&action=getStatus&id=ID
        if (CodeReceived)
            await SetStatus("3");

        while (!_disposed)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await DisposeAsync();
                throw new TaskCanceledException();
            }

            var responseMessage = await Client.GetAsync($"{_options.BaseUrl}?" +
                                                        $"api_key={_options.ApiToken}" +
                                                        "&action=getStatus" +
                                                        $"&id={Id}", cancellationToken);
            var response = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var data = response.Split(':');
            try
            {
                if (string.Equals(data[0], "STATUS_WAIT_CODE", StringComparison.OrdinalIgnoreCase))
                {
                    await Task.Delay(250, cancellationToken);
                    continue;
                }

                if (string.Equals(data[0], "STATUS_OK", StringComparison.OrdinalIgnoreCase))
                {
                    CodeReceived = true;
                    return data[1];
                }

                if (string.Equals(data[0], "STATUS_CANCEL", StringComparison.OrdinalIgnoreCase))
                    throw new SmsHubActivatorException(data[0]);
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new SmsHubActivatorException(response);
            }
        }

        throw new SmsHubActivatorException("");
    }

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        if (CodeReceived)
            await SetStatus("6");
        else
            await SetStatus("8");
    }

    private async Task SetStatus(string status)
    {
        //https://smshub.org/stubs/handler_api.php?api_key=APIKEY&action=setStatus&status=STATUS&id=ID
        var responseMessage = await Client.GetAsync($"{_options.BaseUrl}?" +
                                                    $"api_key={_options.ApiToken}" +
                                                    "&action=setStatus" +
                                                    $"&status={status}" +
                                                    $"&id={Id}");
        var response = await responseMessage.Content.ReadAsStringAsync();
        var data = response.Split(':');
        if (!data[0].StartsWith("ACCESS"))
            throw new SmsHubActivatorException(response);
    }
}