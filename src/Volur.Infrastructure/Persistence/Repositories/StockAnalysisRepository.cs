using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volur.Application.Interfaces;
using Volur.Domain.Entities;
using Volur.Infrastructure.Persistence;

namespace Volur.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for stock analysis data.
/// </summary>
public sealed class StockAnalysisRepository : IStockAnalysisRepository
{
    private readonly VolurDbContext _context;
    private readonly ILogger<StockAnalysisRepository> _logger;

    public StockAnalysisRepository(VolurDbContext context, ILogger<StockAnalysisRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Stock Notes
    public async Task<IEnumerable<StockNote>> GetNotesAsync(string ticker, string exchangeCode, CancellationToken cancellationToken = default)
    {
        // Soft delete is handled by query filter in DbContext
        return await _context.StockNotes
            .Where(n => n.Ticker == ticker && n.ExchangeCode == exchangeCode)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<StockNote?> GetNoteByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.StockNotes.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<StockNote> CreateNoteAsync(StockNote note, CancellationToken cancellationToken = default)
    {
        // Timestamps managed by interceptor
        _context.StockNotes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);
        
        return note;
    }

    public async Task<StockNote> UpdateNoteAsync(StockNote note, CancellationToken cancellationToken = default)
    {
        // Timestamps managed by interceptor
        _context.StockNotes.Update(note);
        await _context.SaveChangesAsync(cancellationToken);
        
        return note;
    }

    public async Task DeleteNoteAsync(int id, CancellationToken cancellationToken = default)
    {
        var note = await _context.StockNotes.FindAsync(new object[] { id }, cancellationToken);
        if (note != null)
        {
            note.SoftDelete(); // Soft delete managed by interceptor
            _context.StockNotes.Update(note);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // Stock Key-Values
    public async Task<IEnumerable<StockKeyValue>> GetKeyValuesAsync(string ticker, string exchangeCode, CancellationToken cancellationToken = default)
    {
        // Soft delete is handled by query filter in DbContext
        return await _context.StockKeyValues
            .Where(kv => kv.Ticker == ticker && kv.ExchangeCode == exchangeCode)
            .OrderByDescending(kv => kv.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<StockKeyValue?> GetKeyValueByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.StockKeyValues.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<StockKeyValue> CreateKeyValueAsync(StockKeyValue keyValue, CancellationToken cancellationToken = default)
    {
        // Timestamps managed by interceptor
        _context.StockKeyValues.Add(keyValue);
        await _context.SaveChangesAsync(cancellationToken);
        
        return keyValue;
    }

    public async Task<StockKeyValue> UpdateKeyValueAsync(StockKeyValue keyValue, CancellationToken cancellationToken = default)
    {
        // Timestamps managed by interceptor
        _context.StockKeyValues.Update(keyValue);
        await _context.SaveChangesAsync(cancellationToken);
        
        return keyValue;
    }

    public async Task DeleteKeyValueAsync(int id, CancellationToken cancellationToken = default)
    {
        var keyValue = await _context.StockKeyValues.FindAsync(new object[] { id }, cancellationToken);
        if (keyValue != null)
        {
            keyValue.SoftDelete(); // Soft delete managed by interceptor
            _context.StockKeyValues.Update(keyValue);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
