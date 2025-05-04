namespace CurrencyConverter.Domain.Interfaces
{
    public interface ICurrencyValidationService
    {
        bool IsRestrictedCurrency(string currency);
        IEnumerable<string> GetRestrictedCurrencies();
        IDictionary<string, decimal> FilterRestrictedCurrencies(IDictionary<string, decimal> rates);
    }
}