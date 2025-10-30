using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        services.Configure<EodhdOptions>(configuration.GetSection(EodhdOptions.SectionName));
        services.Configure<SqlServerOptions>(configuration.GetSection(SqlServerOptions.SectionName));

        // SQL Server EF Core with interceptor for timestamp management
        var sqlServerOptions = configuration.GetSection(SqlServerOptions.SectionName).Get<SqlServerOptions>();
        services.AddDbContext<VolurDbContext>(options =>
        {
            options.UseSqlServer(sqlServerOptions?.ConnectionString ?? "Server=.\\SQLEXPRESS;Database=Volur;Trusted_Connection=True;TrustServerCertificate=True;");
            options.AddInterceptors(new TimestampSaveChangesInterceptor());
        });

        // Repositories
        services.AddScoped<IExchangeRepository, ExchangeRepository>();
        services.AddScoped<ISymbolRepository, SymbolRepository>();
        services.AddScoped<IStockDataRepository, StockDataRepository>();
        services.AddScoped<IStockAnalysisRepository, StockAnalysisRepository>();

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

