namespace Volur.Application.UseCases.BulkFetchFundamentals;

/// <summary>
/// Command to bulk fetch fundamental data for symbols without cached data.
/// </summary>
public sealed record BulkFetchFundamentalsCommand(
    string ExchangeCode,
    int BatchSize = 3000
);
