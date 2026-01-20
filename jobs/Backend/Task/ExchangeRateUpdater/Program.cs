using ExchangeRateUpdater.Application.Providers;
using ExchangeRateUpdater.Core.Entities;
using ExchangeRateUpdater.Data;
using ExchangeRateUpdater.Infrastructure.Options;
using ExchangeRateUpdater.Infrastructure.Policies;
using ExchangeRateUpdater.Infrastructure.Sources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExchangeRateUpdater
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                logging.AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<CnbApiOptions>(context.Configuration.GetSection("CnbApi"));

                services.AddHttpClient<CnbApiExchangeRateSource>((sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<CnbApiOptions>>().Value;
                    client.BaseAddress = new Uri(options.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(10);
                })
                .AddPolicyHandler(CnbHttpPolicies.RetryPolicy);

                services.AddScoped<IExchangeRateSource>(sp => sp.GetRequiredService<CnbApiExchangeRateSource>());
                services.AddTransient<IExchangeRateProvider, ExchangeRateProvider>();
            })
            .Build();

            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ExchangeRateUpdater");
            logger.LogInformation("Getting lastest exchange rates...");

            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            IExchangeRateProvider exchangeRateProvider = services.GetRequiredService<IExchangeRateProvider>();

            try
            {
                IEnumerable<Currency> currencies = SupportedCurrencies.All;
                IEnumerable<ExchangeRate> rates = await exchangeRateProvider.GetExchangeRates(currencies);

                if (rates == null || rates.Count() <= 0)
                {
                    logger.LogWarning("No exchange rates were retrieved.");
                }
                else
                {
                    logger.LogInformation($"Successfully retrieved {rates.Count()} exchange rates:");
                    foreach (ExchangeRate rate in rates)
                    {
                        logger.LogInformation(rate.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Could not retrieve exchange rates: {e.Message}");
            }

            Console.ReadLine();

            await host.StopAsync();
        }
    }
}
