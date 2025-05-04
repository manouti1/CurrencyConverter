using CurrencyConverter.Domain.Interfaces;
using CurrencyConverter.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace CurrencyConverter.Infrastructure.Providers;

public class FrankfurterProvider : IExchangeProvider
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheTtl;
    private const string ApiPrefix = "/v1";

    public string Name => "Frankfurter";

    public FrankfurterProvider(HttpClient http, IMemoryCache cache, IConfiguration configuration)
    {
        _http = http;
        _cache = cache;
        _cacheTtl = TimeSpan.FromSeconds(configuration.GetValue<int>("CacheSettings:DurationSeconds"));
    }

    public async Task<ExchangeRate> GetLatestAsync(string baseCurrency, CancellationToken ct = default)
    {
        // Endpoint: GET /v1/latest?base={baseCurrency}
        var cacheKey = $"latest_{baseCurrency}";
        if (_cache.TryGetValue(cacheKey, out ExchangeRate cached))
            return cached;

        var exchange = await FetchAndParseAsync($"{ApiPrefix}/latest?base={baseCurrency}", ct);
        _cache.Set(cacheKey, exchange, _cacheTtl);
        return exchange;
    }

    public async Task<ExchangeRate> GetHistoricalAsync(string baseCurrency, DateTime date, CancellationToken ct = default)
    {
        // Endpoint: GET /v1/{yyyy-MM-dd}?base={baseCurrency}
        var dateSegment = date.ToString("yyyy-MM-dd");
        var cacheKey = $"hist_{baseCurrency}_{dateSegment}";
        if (_cache.TryGetValue(cacheKey, out ExchangeRate cached))
            return cached;

        var exchange = await FetchAndParseAsync($"{ApiPrefix}/{dateSegment}?base={baseCurrency}", ct);
        _cache.Set(cacheKey, exchange, _cacheTtl);
        return exchange;
    }

    private async Task<ExchangeRate> FetchAndParseAsync(string url, CancellationToken ct)
    {
        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var json = await JsonSerializer.DeserializeAsync<JsonElement>(stream, cancellationToken: ct);

        var date = json.GetProperty("date").GetDateTime();
        var baseCur = json.GetProperty("base").GetString()!;
        var rates = json.GetProperty("rates")
                          .EnumerateObject()
                          .ToDictionary(p => p.Name, p => p.Value.GetDecimal());

        return new ExchangeRate
        {
            Date = date,
            BaseCurrency = baseCur,
            Rates = rates
        };
    }
}
