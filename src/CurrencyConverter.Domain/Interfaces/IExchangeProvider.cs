using CurrencyConverter.Domain.Models;

namespace CurrencyConverter.Domain.Interfaces;

public interface IExchangeProvider
{
    string Name { get; }
    Task<ExchangeRate> GetLatestAsync(string baseCurrency, CancellationToken ct = default);
    Task<ExchangeRate> GetHistoricalAsync(string baseCurrency, DateTime date, CancellationToken ct = default);
}