using CurrencyConverter.Application.Factories;
using CurrencyConverter.Domain.Interfaces;
using CurrencyConverter.Domain.Models;

namespace CurrencyConverter.Application.Services;

public class ExchangeService : IExchangeService
{
    private readonly IExchangeProviderFactory _factory;
    private readonly ICurrencyValidationService _currencyValidationService;
    public ExchangeService(IExchangeProviderFactory factory, ICurrencyValidationService currencyValidationService)
    {
        _factory = factory;
        _currencyValidationService = currencyValidationService;
    }

    public async Task<decimal> ConvertAsync(string from, string to, decimal amount, CancellationToken ct = default)
    {
        if (_currencyValidationService.IsRestrictedCurrency(from) ||
    _currencyValidationService.IsRestrictedCurrency(to))
        {
            var restricted = string.Join(", ", _currencyValidationService.GetRestrictedCurrencies());
            throw new InvalidOperationException($"Conversions involving restricted currencies ({restricted}) are not allowed.");
        }

        if (from.Equals(to, StringComparison.OrdinalIgnoreCase)) return amount;
        var provider = _factory.GetProvider("Frankfurter");
        var rates = await provider.GetLatestAsync(from, ct);
        if (!rates.Rates.TryGetValue(to, out var rate))
            throw new KeyNotFoundException($"Currency '{to}' not found.");
        return Math.Round(amount * rate, 4);
    }

    public Task<ExchangeRate> GetLatestRatesAsync(string baseCurrency, CancellationToken ct = default)
    {
        if (_currencyValidationService.IsRestrictedCurrency(baseCurrency))
        {
            var restricted = string.Join(", ", _currencyValidationService.GetRestrictedCurrencies());
            throw new InvalidOperationException($"Rate queries for restricted currencies ({restricted}) are not allowed.");
        }
        return _factory.GetProvider("Frankfurter").GetLatestAsync(baseCurrency, ct);
    }

    public async Task<PagedList<ExchangeRate>> GetHistoryAsync(string baseCurrency, DateTime from, DateTime to, PaginationParams pagination, CancellationToken ct = default)
    {
        if (_currencyValidationService.IsRestrictedCurrency(baseCurrency))
        {
            var restricted = string.Join(", ", _currencyValidationService.GetRestrictedCurrencies());
            throw new InvalidOperationException($"Historical rates for restricted currencies ({restricted}) are not allowed.");
        }

         var provider = _factory.GetProvider("Frankfurter");
        var allDates = Enumerable.Range(0, (to - from).Days + 1)
            .Select(offset => from.Date.AddDays(offset));

        var totalCount = allDates.Count();
        var skipAmount = (pagination.PageNumber - 1) * pagination.PageSize;
        var pagedDates = allDates
            .Skip(skipAmount)
            .Take(pagination.PageSize);

        var rates = new List<ExchangeRate>();
        foreach (var date in pagedDates)
        {
            rates.Add(await provider.GetHistoricalAsync(baseCurrency, date, ct));
        }

        return new PagedList<ExchangeRate>(rates, totalCount, pagination.PageNumber, pagination.PageSize);
    }
}