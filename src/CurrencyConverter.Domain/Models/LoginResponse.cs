namespace CurrencyConverter.Domain.Models
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public DateTime Expiration { get; set; }
    }
}