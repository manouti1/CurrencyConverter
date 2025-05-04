using CurrencyConverter.Domain.Interfaces;

namespace CurrencyConverter.Application.Services
{
    public class CurrencyValidationService : ICurrencyValidationService
    {
        private readonly HashSet<string> _restrictedCurrencies = new HashSet<string> { "TRY", "PLN", "THB", "MXN" };

        public bool IsRestrictedCurrency(string currency)
        {
            return _restrictedCurrencies.Contains(currency);
        }

        public IEnumerable<string> GetRestrictedCurrencies()
        {
            return _restrictedCurrencies;
        }

        public IDictionary<string, decimal> FilterRestrictedCurrencies(IDictionary<string, decimal> rates)
        {
            return rates.Where(rate => !_restrictedCurrencies.Contains(rate.Key))
                        .ToDictionary(rate => rate.Key, rate => rate.Value);
        }
    }
}
