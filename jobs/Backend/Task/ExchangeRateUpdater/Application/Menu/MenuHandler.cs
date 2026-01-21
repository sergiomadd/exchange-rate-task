using ExchangeRateUpdater.Application.Providers;
using ExchangeRateUpdater.Application.Validators;
using ExchangeRateUpdater.Core.Entities;
using ExchangeRateUpdater.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateUpdater.Application.Menu
{
    namespace ExchangeRateUpdater.Application.Menu
    {
        public class MenuHandler
        {
            private readonly IExchangeRateProvider _exchangeRateProvider;
            private readonly ILogger<MenuHandler> _logger;

            public MenuHandler(IExchangeRateProvider exchangeRateProvider, ILogger<MenuHandler> logger)
            {
                _exchangeRateProvider = exchangeRateProvider;
                _logger = logger;
            }

            public async Task RunAsync()
            {
                //Needed cause async method -> console initializes before app sometimes
                Console.WriteLine("Starting Exchange Rate Updater...");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();

                bool exit = false;

                while (!exit)
                {
                    Console.Clear();
                    Console.WriteLine("=== Exchange Rate Updater ===");
                    Console.WriteLine("[1] Get latest exchange rates");
                    Console.WriteLine("[0] Exit");
                    Console.Write("Choose an option: ");

                    string choice = Console.ReadLine();

                    bool actionExecuted = false;

                    switch (choice)
                    {
                        case "1":
                            await GetLatestExchangeRates();
                            actionExecuted = true;
                            break;

                        case "0":
                            exit = true;
                            break;

                        default:
                            Console.WriteLine("Invalid option");
                            break;
                    }

                    if (actionExecuted && !exit)
                    {
                        Console.WriteLine("\nPress Enter to continue...");
                        Console.ReadLine();
                    }
                }
            }

            private async Task GetLatestExchangeRates()
            {
                _logger.LogInformation("Getting latest exchange rates...");

                try
                {
                    IEnumerable<Currency> currencies = SupportedCurrencies.All;
                    IEnumerable<ExchangeRate> rates = await _exchangeRateProvider.GetExchangeRates(currencies);

                    if (rates == null || rates.Count() <= 0)
                    {
                        _logger.LogWarning("No exchange rates were retrieved.");
                    }
                    else
                    {
                        _logger.LogInformation($"Successfully retrieved {rates.Count()} exchange rates:");
                        foreach (ExchangeRate rate in rates)
                        {
                            _logger.LogInformation(rate.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"Could not retrieve exchange rates: {e.Message}");
                }
            }
        }
    }

}
