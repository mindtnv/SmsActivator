using Microsoft.Extensions.Options;
using SmsActivator.Abstractions;

namespace SmsActivator.SmsHub;

public class SmsHubSmsActivator : ISmsActivator
{
    private readonly SmsHubSmsActivatorOptions _options;
    private readonly IServiceNameResolver _serviceNameResolver;
    private readonly HttpClient _client;

    public SmsHubSmsActivator(
        HttpClient client,
        IOptions<SmsHubSmsActivatorOptions> options,
        IServiceNameResolver serviceNameResolver
    )
    {
        _client = client;
        _serviceNameResolver = serviceNameResolver;
        _options = options.Value;
    }

    public async Task<IActivationManager> ActivateAsync(ActivationParameters parameters)
    {
        if (_options.ApiToken == null)
            throw new ArgumentNullException(nameof(_options.ApiToken));
        if (_options.BaseUrl == null)
            throw new ArgumentNullException(nameof(_options.BaseUrl));

        var responseMessage = await _client.GetAsync(
            $"{_options.BaseUrl}?api_key={_options.ApiToken}" +
            $"&action=getNumber&service={_serviceNameResolver.ResolveServiceName(parameters.Service)}" +
            $"&operator={parameters.Operator}" +
            $"&country={parameters.Country}"
        );
        var response = await responseMessage.Content.ReadAsStringAsync();
        var data = response.Split(':');
        try
        {
            if (string.Compare(data[0], "ACCESS_NUMBER", StringComparison.OrdinalIgnoreCase) == 0)
                return new SmsHubActivationManager(data[2], data[1], parameters, _options);
        }
        catch (Exception)
        {
            throw new SmsHubActivatorException(response);
        }

        throw new SmsHubActivatorException(response);
    }
}