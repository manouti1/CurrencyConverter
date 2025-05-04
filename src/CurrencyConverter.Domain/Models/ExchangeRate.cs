namespace CurrencyConverter.Domain.Models;

public class ExchangeRate
{
    public string BaseCurrency { get; init; } = default!;
    public DateTime Date { get; init; }
    public IReadOnlyDictionary<string, decimal> Rates { get; init; } = default!;
}