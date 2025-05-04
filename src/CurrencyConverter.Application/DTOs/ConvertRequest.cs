namespace CurrencyConverter.Application.DTOs;

public record ConvertRequest(string From, string To, decimal Amount);