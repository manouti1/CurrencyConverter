using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace CurrencyConverter.Tests.Integration
{
    public class CurrencyControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public CurrencyControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<string> GetJwtTokenAsync()
        {
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "testpass@123!"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/Auth/login", loginRequest);
            response.EnsureSuccessStatusCode();
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return loginResponse.Token;
        }

        [Fact]
        public async Task Convert_WithValidToken_ReturnsConvertedAmount()
        {
            // Arrange
            var token = await GetJwtTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            ConvertRequest request = new("USD", "EUR", 100);

            // Act
            var response = await _client.GetAsync($"/api/v1/Currency/convert?from={request.From}&to={request.To}&amount={request.Amount}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var convertResponse = await response.Content.ReadFromJsonAsync<ConvertResponse>();
            convertResponse.Should().NotBeNull();
            convertResponse.From.Should().Be("USD");
            convertResponse.To.Should().Be("EUR");
            convertResponse.ConvertedAmount.Should().Be(100);
        }

        [Fact]
        public async Task Convert_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            ConvertRequest request = new
            (
                "USD",
                "EUR",
                100
            );

            // Act
            var response = await _client.GetAsync($"/api/v1/Currency/convert?from={request.From}&to={request.To}&amount={request.Amount}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task LatestRates_ReturnsRates()
        {
            // Arrange
            var response = await _client.GetAsync("/api/v1/Currency/rates/latest?baseCurrency=USD");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var rates = await response.Content.ReadFromJsonAsync<ExchangeRate>();
            rates.Should().NotBeNull();
            rates.BaseCurrency.Should().Be("USD");
            rates.Rates.Should().NotBeEmpty();
        }
    }
}