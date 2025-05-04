using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using CurrencyConverter.Application.Services;
using CurrencyConverter.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CurrencyConverter.Tests.Unit
{
    public class AuthServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(x => x["Jwt:UserName"]).Returns("testuser");
            _configMock.Setup(x => x["Jwt:Password"]).Returns("testpass");
            _configMock.Setup(x => x["Jwt:Key"]).Returns("supersecretkeysupersecretkey1234");
            _configMock.Setup(x => x["Jwt:Issuer"]).Returns("test-issuer");
            _configMock.Setup(x => x["Jwt:Audience"]).Returns("test-audience");
            _configMock.Setup(x => x["Jwt:ExpirationMinutes"]).Returns("60");

            _authService = new AuthService(_configMock.Object);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var request = new LoginRequest { Username = "testuser", Password = "testpass" };

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.Username.Should().Be("testuser");

            // Validate JWT
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Token);
            jwt.Claims.Should().Contain(c => c.Type == "permission" && c.Value == "Convert");
            jwt.Claims.Should().Contain(c => c.Type == "permission" && c.Value == "History");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var request = new LoginRequest { Username = "wrong", Password = "wrong" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
        }
    }
}