using ExchangeRateUpdater.Application.DTOs;
using ExchangeRateUpdater.Core.Entities;
using ExchangeRateUpdater.Data;
using ExchangeRateUpdater.Infrastructure.Sources;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExchangeRateUpdater.Application.Providers
{
    public class ExchangeRateProvider : IExchangeRateProvider
    {
        private readonly ILogger<ExchangeRateProvider> _logger;
        private readonly IExchangeRateSource _source;
        public ExchangeRateProvider(ILogger<ExchangeRateProvider> logger, IExchangeRateSource source)
        {
            _logger = logger;
            _source = source;
        }

        /// <summary>
        /// Should return exchange rates among the specified currencies that are defined by the source. But only those defined
        /// by the source, do not return calculated exchange rates. E.g. if the source contains "CZK/USD" but not "USD/CZK",
        /// do not return exchange rate "USD/CZK" with value calculated as 1 / "CZK/USD". If the source does not provide
        /// some of the currencies, ignore them.
        /// </summary>
        public async Task<IEnumerable<ExchangeRate>> GetExchangeRates(IEnumerable<Currency> currencies)
        {
            IEnumerable<ExchangeRateDTO> rates = await _source.GetDailyExchangeRates();
            List<ExchangeRate> validRates = new List<ExchangeRate>();

            //Used foreach instead of linq to be able to log skipped invalid rates
            foreach (ExchangeRateDTO exchangeRateDTO in rates.Where(dto => dto != null && currencies.Any(c => c.Code == dto.CurrencyCode)))
            {
                //Skip rates where the currency equals the base currency
                if (exchangeRateDTO.CurrencyCode == SupportedCurrencies.DefaultBaseCurrency.Code)
                {
                    _logger.LogInformation($"Skipped exchange rate for base currency {exchangeRateDTO.CurrencyCode}");
                    continue;
                }
                if (IsExchangeRateDTOValid(exchangeRateDTO))
                {
                    validRates.Add(ExchangeRateMapDTOToModel(exchangeRateDTO));
                }
                else
                {
                    _logger.LogWarning($"Skipped invalid exchange rate for Currency={exchangeRateDTO.CurrencyCode} with Amount={exchangeRateDTO.Amount}, Rate={exchangeRateDTO.Rate}");
                }
            }

            return validRates;
        }

        private static bool IsExchangeRateDTOValid(ExchangeRateDTO exchangeRateDTO)
        {
            return !string.IsNullOrWhiteSpace(exchangeRateDTO.CurrencyCode)
                && exchangeRateDTO.Rate > 0
                && exchangeRateDTO.Amount > 0;
        }

        
        private static ExchangeRate ExchangeRateMapDTOToModel(ExchangeRateDTO exchangeRateDTO)
        {
            //Nomalize rate incase amount given is greater than 1
            decimal normalizedRate = exchangeRateDTO.Amount > 1 ? exchangeRateDTO.Rate / exchangeRateDTO.Amount : exchangeRateDTO.Rate;
            return new ExchangeRate(SupportedCurrencies.DefaultBaseCurrency, new Currency(exchangeRateDTO.CurrencyCode), normalizedRate);
        }
    }
}
