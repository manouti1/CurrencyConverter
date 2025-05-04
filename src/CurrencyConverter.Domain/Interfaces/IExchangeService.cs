using CurrencyConverter.Domain.Models;

namespace CurrencyConverter.Domain.Interfaces;

public interface IExchangeService
{
    Task<decimal> ConvertAsync(string from, string to, decimal amount, CancellationToken ct = default);
    Task<ExchangeRate> GetLatestRatesAsync(string baseCurrency, CancellationToken ct = default);
     Task<PagedList<ExchangeRate>> GetHistoryAsync(string baseCurrency, DateTime from, DateTime to, PaginationParams pagination, CancellationToken ct = default);
}