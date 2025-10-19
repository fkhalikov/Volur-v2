using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volur.Application.Configuration;
using Volur.Application.UseCases.GetExchanges;
using Volur.Application.UseCases.GetSymbols;
using Volur.Application.UseCases.RefreshSymbols;
using Volur.Application.UseCases.GetStockQuote;
using Volur.Application.UseCases.GetStockFundamentals;
using Volur.Application.UseCases.GetHistoricalPrices;
using Volur.Application.UseCases.GetStockDetails;
using Volur.Application.UseCases.BulkFetchFundamentals;

namespace Volur.Application;

/// <summary>
/// Dependency injection configuration for Application layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<CacheTtlOptions>(configuration.GetSection(CacheTtlOptions.SectionName));

        // Handlers
        services.AddScoped<GetExchangesHandler>();
        services.AddScoped<GetSymbolsHandler>();
        services.AddScoped<RefreshSymbolsHandler>();
        services.AddScoped<GetStockQuoteHandler>();
        services.AddScoped<GetStockFundamentalsHandler>();
        services.AddScoped<GetHistoricalPricesHandler>();
        services.AddScoped<GetStockDetailsHandler>();
        services.AddScoped<BulkFetchFundamentalsHandler>();

        // Validators
        services.AddValidatorsFromAssemblyContaining<GetSymbolsQuery>();

        return services;
    }
}

