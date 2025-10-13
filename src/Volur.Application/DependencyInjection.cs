using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volur.Application.Configuration;
using Volur.Application.UseCases.GetExchanges;
using Volur.Application.UseCases.GetSymbols;
using Volur.Application.UseCases.RefreshSymbols;

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

        // Validators
        services.AddValidatorsFromAssemblyContaining<GetSymbolsQuery>();

        return services;
    }
}

