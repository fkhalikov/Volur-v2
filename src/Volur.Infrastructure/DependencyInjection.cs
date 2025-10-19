using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volur.Application.Configuration;
using Volur.Application.Interfaces;
using Volur.Infrastructure.Configuration;
using Volur.Infrastructure.ExternalProviders;
using Volur.Infrastructure.Persistence;
using Volur.Infrastructure.Persistence.Repositories;

namespace Volur.Infrastructure;

/// <summary>
/// Dependency injection configuration for Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.SectionName));
        services.Configure<EodhdOptions>(configuration.GetSection(EodhdOptions.SectionName));

        // MongoDB
        services.AddSingleton<MongoDbContext>();

        // Repositories
        services.AddScoped<IExchangeRepository, ExchangeRepository>();
        services.AddScoped<ISymbolRepository, SymbolRepository>();
        services.AddScoped<IStockDataRepository, StockDataRepository>();

        // Stock Data Provider
        services.AddScoped<IStockDataProvider, EodhdStockDataProvider>();

        // EODHD HTTP Client
        var eodhdOptions = configuration.GetSection(EodhdOptions.SectionName).Get<EodhdOptions>();
        services.AddHttpClient<IEodhdClient, EodhdClient>(client =>
        {
            client.BaseAddress = new Uri(eodhdOptions?.BaseUrl ?? "https://eodhd.com/");
            client.Timeout = TimeSpan.FromSeconds(eodhdOptions?.TimeoutSeconds ?? 10);
        })
        .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
        .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy());

        return services;
    }
}

