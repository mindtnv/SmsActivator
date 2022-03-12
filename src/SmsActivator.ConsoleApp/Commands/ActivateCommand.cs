using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using SmsActivator.Abstractions;

namespace SmsActivator.ConsoleApp.Commands;

public class ActivateCommand : RootCommand
{
    public ActivateCommand() : base("Activate new phone number for service")
    {
        AddArgument(new Argument<string>("Service", "Service to activate"));
    }

    public new class Handler : ICommandHandler
    {
        private readonly ILogger<Handler> _logger;
        private readonly ISmsActivator _smsActivator;
        public string? Service { get; set; }

        public Handler(ISmsActivator smsActivator, ILogger<Handler> logger)
        {
            _smsActivator = smsActivator;
            _logger = logger;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            await using var manager = await _smsActivator.ActivateAsync(new ActivationParameters
            {
                Service = Service ?? throw new ArgumentException(nameof(Service)),
            });
            var cancellationToken = context.GetCancellationToken();
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                try
                {
                    _logger.LogInformation("[{Service}] {Phone} - waiting code...", Service, manager.Phone);
                    var code = await manager.GetCodeAsync(cancellationToken);
                    var needAnotherCode = false;
                    while (true)
                    {
                        _logger.LogInformation("[{Service}] {Phone} - {Code}. Need another one? Y/n",
                            Service, manager.Phone, code);
                        var key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.N)
                            break;
                        if (key == ConsoleKey.Y)
                        {
                            needAnotherCode = true;
                            break;
                        }
                    }

                    if (!needAnotherCode)
                        break;
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("[{Service}] {Phone} - canceling", Service, manager.Phone);
                }
                catch (Exception)
                {
                    return -1;
                }
            }

            return 0;
        }
    }
}