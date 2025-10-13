namespace Volur.Application.UseCases.RefreshSymbols;

/// <summary>
/// Command to force refresh symbols for an exchange.
/// </summary>
public sealed record RefreshSymbolsCommand(string ExchangeCode);

