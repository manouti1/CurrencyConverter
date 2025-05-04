namespace CurrencyConverter.Application.DTOs;

public record ConvertResponse(string From, string To, decimal OriginalAmount, decimal ConvertedAmount);