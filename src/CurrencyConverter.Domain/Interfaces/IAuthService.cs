using CurrencyConverter.Domain.Models;

namespace CurrencyConverter.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}