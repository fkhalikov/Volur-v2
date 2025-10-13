namespace Volur.Application.UseCases.GetExchanges;

/// <summary>
/// Query to get all exchanges.
/// </summary>
public sealed record GetExchangesQuery(bool ForceRefresh = false);

