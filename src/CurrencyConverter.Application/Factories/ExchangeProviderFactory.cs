using CurrencyConverter.Domain.Interfaces;

namespace CurrencyConverter.Application.Factories;

public interface IExchangeProviderFactory
{
    IExchangeProvider GetProvider(string name);
}

public class ExchangeProviderFactory : IExchangeProviderFactory
{
    private readonly IEnumerable<IExchangeProvider> _providers;
    public ExchangeProviderFactory(IEnumerable<IExchangeProvider> providers) => _providers = providers;

    public IExchangeProvider GetProvider(string name)
        => _providers.Single(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}