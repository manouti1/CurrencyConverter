using CurrencyConverter.Domain.Interfaces;
using CurrencyConverter.Infrastructure.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace CurrencyConverter.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.AddHttpClient<IExchangeProvider, FrankfurterProvider>(client =>
        {
            client.BaseAddress = new Uri(configuration["Frankfurter:BaseUrl"]);
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        => HttpPolicyExtensions.HandleTransientHttpError()
                               .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        => HttpPolicyExtensions.HandleTransientHttpError()
                               .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}