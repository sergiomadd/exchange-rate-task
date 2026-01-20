using ExchangeRateUpdater.Application.DTOs;
using ExchangeRateUpdater.Application.Providers;
using ExchangeRateUpdater.Core.Entities;
using ExchangeRateUpdater.Core.Exceptions;
using ExchangeRateUpdater.Infrastructure.Sources;
using ExchangeRateUpdater.Tests.UnitTests.Data;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ExchangeRateUpdater.Tests.UnitTests
{
    public class ExchangeRateProviderTests
    {
        private readonly Mock<IExchangeRateSource> _sourceMock;
        private readonly Mock<ILogger<ExchangeRateProvider>> _loggerMock;

        public ExchangeRateProviderTests()
        {
            _sourceMock = new Mock<IExchangeRateSource>();
            _loggerMock = new Mock<ILogger<ExchangeRateProvider>>();
        }

        [Fact]
        public async Task GetExchangeRates_ReturnsOnlyRatesForRequestedCurrencies()
        {
            // Arrange
            _sourceMock.Setup(s => s.GetDailyExchangeRates()).ReturnsAsync(ExchangeRateProviderTestData.ValidRates);

            var provider = new ExchangeRateProvider(_loggerMock.Object, _sourceMock.Object);

            var currencies = new[] { new Currency("USD") };

            // Act
            var result = (await provider.GetExchangeRates(currencies)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("USD", result[0].TargetCurrency.Code);
        }

        [Fact]
        public async Task GetExchangeRates_IgnoresRequestedCurrenciesNotInSource()
        {
            // Arrange
            _sourceMock.Setup(s => s.GetDailyExchangeRates()).ReturnsAsync(ExchangeRateProviderTestData.ValidRates);

            var provider = new ExchangeRateProvider(_loggerMock.Object, _sourceMock.Object);

            var currencies = new[]
            {
                new Currency("USD"),
                new Currency("GBP")
            };

            // Act
            var result = (await provider.GetExchangeRates(currencies)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("USD", result[0].TargetCurrency.Code);
        }

        [Fact]
        public async Task GetExchangeRates_SkipsInvalidExchangeRateDTOs()
        {
            // Arrange
            _sourceMock.Setup(s => s.GetDailyExchangeRates()).ReturnsAsync(ExchangeRateProviderTestData.WithInvalidRate);

            var provider = new ExchangeRateProvider(_loggerMock.Object, _sourceMock.Object);

            var currencies = new[]
            {
                new Currency("USD"),
                new Currency("EUR")
            };

            // Act
            var result = (await provider.GetExchangeRates(currencies)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("EUR", result[0].TargetCurrency.Code);
        }

        [Fact]
        public async Task GetExchangeRates_NormalizesRate_WhenAmountIsGreaterThanOne()
        {
            // Arrange
            _sourceMock.Setup(s => s.GetDailyExchangeRates()).ReturnsAsync(ExchangeRateProviderTestData.WithAmountGreaterThanOne);

            var provider = new ExchangeRateProvider(_loggerMock.Object, _sourceMock.Object);

            // Act
            var result = (await provider.GetExchangeRates(new[] { new Currency("USD") })).Single();

            // Assert
            Assert.Equal(22m, result.Value);
        }

        [Fact]
        public async Task GetExchangeRates_SkipsNullExchangeRateDTOs()
        {
            // Arrange
            _sourceMock.Setup(s => s.GetDailyExchangeRates()).ReturnsAsync(ExchangeRateProviderTestData.WithNullAndInvalidEntries);

            var provider = new ExchangeRateProvider(_loggerMock.Object, _sourceMock.Object);

            // Act
            var result = (await provider.GetExchangeRates(new[] { new Currency("USD") })).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("USD", result[0].TargetCurrency.Code);
        }

        [Fact]
        public async Task GetExchangeRates_SkipsBaseCurrencyDTOs()
        {
            // Arrange
            _sourceMock.Setup(s => s.GetDailyExchangeRates()).ReturnsAsync(ExchangeRateProviderTestData.WithBaseCurrency);

            var provider = new ExchangeRateProvider(_loggerMock.Object, _sourceMock.Object);

            var currencies = new[]
            {
                new Currency("USD"),
                new Currency("CZK"),
                new Currency("EUR")
            };

            // Act
            var result = (await provider.GetExchangeRates(currencies)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("USD", result[0].TargetCurrency.Code);
        }

        [Fact]
        public async Task GetExchangeRates_Throws_WhenSourceThrows()
        {
            // Arrange
            _sourceMock.Setup(s => s.GetDailyExchangeRates()).ThrowsAsync(new ExchangeRateSourceException("Failure"));

            var provider = new ExchangeRateProvider(_loggerMock.Object, _sourceMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ExchangeRateSourceException>(() => provider.GetExchangeRates(new[] { new Currency("USD") }));
        }
    }
}
