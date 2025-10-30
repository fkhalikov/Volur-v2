namespace Volur.Application.Interfaces;

/// <summary>
/// Repository for managing stock analysis data (notes and key-value pairs).
/// </summary>
public interface IStockAnalysisRepository
{
    // Stock Notes
    Task<IEnumerable<Domain.Entities.StockNote>> GetNotesAsync(string ticker, string exchangeCode, CancellationToken cancellationToken = default);
    Task<Domain.Entities.StockNote?> GetNoteByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Domain.Entities.StockNote> CreateNoteAsync(Domain.Entities.StockNote note, CancellationToken cancellationToken = default);
    Task<Domain.Entities.StockNote> UpdateNoteAsync(Domain.Entities.StockNote note, CancellationToken cancellationToken = default);
    Task DeleteNoteAsync(int id, CancellationToken cancellationToken = default);

    // Stock Key-Values
    Task<IEnumerable<Domain.Entities.StockKeyValue>> GetKeyValuesAsync(string ticker, string exchangeCode, CancellationToken cancellationToken = default);
    Task<Domain.Entities.StockKeyValue?> GetKeyValueByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Domain.Entities.StockKeyValue> CreateKeyValueAsync(Domain.Entities.StockKeyValue keyValue, CancellationToken cancellationToken = default);
    Task<Domain.Entities.StockKeyValue> UpdateKeyValueAsync(Domain.Entities.StockKeyValue keyValue, CancellationToken cancellationToken = default);
    Task DeleteKeyValueAsync(int id, CancellationToken cancellationToken = default);
}
