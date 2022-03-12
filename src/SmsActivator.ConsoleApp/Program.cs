using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmsActivator.Abstractions;
using SmsActivator.ConsoleApp.Commands;
using SmsActivator.SmsHub;

var builder = new CommandLineBuilder(new ActivateCommand())
              .UseHost(builder =>
              {
                  builder.ConfigureAppConfiguration(configurationBuilder =>
                  {
                      configurationBuilder.AddInMemoryCollection(new[]
                      {
                          new KeyValuePair<string, string?>(
                              ConfigurationPath.Combine("Logging", "LogLevel", "Microsoft.Hosting.Lifetime"), "None"),
                      });
                  });
                  builder.ConfigureServices((context, services) =>
                  {
                      services.Configure<SmsHubSmsActivatorOptions>(
                          context.Configuration.GetSection(nameof(SmsHubSmsActivatorOptions)));
                      services.AddSingleton<IServiceNameResolver, SmsHubServiceNameResolver>();
                      services.AddSingleton<ISmsActivator, SmsHubSmsActivator>();
                      services.AddTransient<Command, ActivateCommand>();
                  });
                  builder.ConfigureDefaults(args);
                  builder.UseCommandHandler<ActivateCommand, ActivateCommand.Handler>();
              })
              .UseDefaults();

await builder.Build().InvokeAsync(args);