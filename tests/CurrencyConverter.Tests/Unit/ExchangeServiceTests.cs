using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CurrencyConverter.Application.Factories;
using CurrencyConverter.Application.Services;
using CurrencyConverter.Domain.Interfaces;
using CurrencyConverter.Domain.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace CurrencyConverter.Tests.Unit
{
    public class ExchangeServiceTests
    {
        private readonly Mock<IExchangeProviderFactory> _factoryMock;
        private readonly Mock<IExchangeProvider> _providerMock;
        private readonly Mock<ICurrencyValidationService> _validationMock;
        private readonly ExchangeService _service;

        public ExchangeServiceTests()
        {
            _factoryMock = new Mock<IExchangeProviderFactory>();
            _providerMock = new Mock<IExchangeProvider>();
            _validationMock = new Mock<ICurrencyValidationService>();

            _factoryMock.Setup(f => f.GetProvider(It.IsAny<string>())).Returns(_providerMock.Object);

            _service = new ExchangeService(_factoryMock.Object, _validationMock.Object);
        }

        [Fact]
        public async Task ConvertAsync_SameCurrency_ReturnsAmount()
        {
            var amount = 123m;
            var result = await _service.ConvertAsync("USD", "USD", amount);
            result.Should().Be(amount);
        }

        [Fact]
        public async Task ConvertAsync_RestrictedCurrency_Throws()
        {
            _validationMock.Setup(v => v.IsRestrictedCurrency("USD")).Returns(true);
            _validationMock.Setup(v => v.GetRestrictedCurrencies()).Returns(new[] { "USD" });

            Func<Task> act = () => _service.ConvertAsync("USD", "EUR", 100);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*restricted currencies*");
        }

        [Fact]
        public async Task ConvertAsync_CurrencyNotFound_Throws()
        {
            _validationMock.Setup(v => v.IsRestrictedCurrency(It.IsAny<string>())).Returns(false);
            _providerMock.Setup(p => p.GetLatestAsync("USD", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ExchangeRate
                {
                    BaseCurrency = "USD",
                    Rates = new Dictionary<string, decimal> { { "GBP", 0.8m } }
                });

            Func<Task> act = () => _service.ConvertAsync("USD", "EUR", 100);
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*Currency 'EUR' not found*");
        }

        [Fact]
        public async Task ConvertAsync_Valid_ReturnsConvertedAmount()
        {
            _validationMock.Setup(v => v.IsRestrictedCurrency(It.IsAny<string>())).Returns(false);
            _providerMock.Setup(p => p.GetLatestAsync("USD", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ExchangeRate
                {
                    BaseCurrency = "USD",
                    Rates = new Dictionary<string, decimal> { { "EUR", 0.9m } }
                });

            var result = await _service.ConvertAsync("USD", "EUR", 100);
            result.Should().Be(90.0m);
        }

        [Fact]
        public async Task GetLatestRatesAsync_RestrictedCurrency_Throws()
        {
            _validationMock.Setup(v => v.IsRestrictedCurrency("USD")).Returns(true);
            _validationMock.Setup(v => v.GetRestrictedCurrencies()).Returns(new[] { "USD" });

            Func<Task> act = () => _service.GetLatestRatesAsync("USD");
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*restricted currencies*");
        }

        [Fact]
        public async Task GetLatestRatesAsync_Valid_ReturnsExchangeRate()
        {
            _validationMock.Setup(v => v.IsRestrictedCurrency("USD")).Returns(false);
            var expected = new ExchangeRate { BaseCurrency = "USD", Rates = new Dictionary<string, decimal> { { "EUR", 0.9m } } };
            _providerMock.Setup(p => p.GetLatestAsync("USD", It.IsAny<CancellationToken>())).ReturnsAsync(expected);

            var result = await _service.GetLatestRatesAsync("USD");
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetHistoryAsync_RestrictedCurrency_Throws()
        {
            _validationMock.Setup(v => v.IsRestrictedCurrency("USD")).Returns(true);
            _validationMock.Setup(v => v.GetRestrictedCurrencies()).Returns(new[] { "USD" });

            var pagination = new PaginationParams { PageNumber = 1, PageSize = 2 };
            Func<Task> act = () => _service.GetHistoryAsync("USD", DateTime.UtcNow.AddDays(-2), DateTime.UtcNow, pagination);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*restricted currencies*");
        }

        [Fact]
        public async Task GetHistoryAsync_Valid_ReturnsPagedList()
        {
            _validationMock.Setup(v => v.IsRestrictedCurrency("USD")).Returns(false);

            var pagination = new PaginationParams { PageNumber = 1, PageSize = 2 };
            var from = DateTime.UtcNow.Date.AddDays(-2);
            var to = DateTime.UtcNow.Date;

            _providerMock.Setup(p => p.GetHistoricalAsync("USD", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string baseCurrency, DateTime date, CancellationToken ct) =>
                    new ExchangeRate { BaseCurrency = baseCurrency, Date = date, Rates = new Dictionary<string, decimal> { { "EUR", 0.9m } } });

            var result = await _service.GetHistoryAsync("USD", from, to, pagination);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(3);
            result.PageNumber.Should().Be(1);
        }
    }
}