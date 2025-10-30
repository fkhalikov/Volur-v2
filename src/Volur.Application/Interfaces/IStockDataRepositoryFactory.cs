namespace Volur.Application.Interfaces;

/// <summary>
/// Factory for creating parallel-safe repository instances.
/// </summary>
public interface IStockDataRepositoryFactory
{
    /// <summary>
    /// Creates a new repository instance with its own DbContext for parallel operations.
    /// </summary>
    IStockDataRepository Create();
}

