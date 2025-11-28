using DomainAgent.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainAgent.Data.Repositories;

/// <summary>
/// Repository for drop list entries.
/// </summary>
public class DropListRepository : IDropListRepository
{
    private readonly DomainAgentDbContext _context;

    public DropListRepository(DomainAgentDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(DropListEntry entry, CancellationToken cancellationToken = default)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        await _context.DropListEntries.AddAsync(entry, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<DropListEntry> entries, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in entries)
        {
            entry.CreatedAt = now;
            entry.UpdatedAt = now;
        }
        await _context.DropListEntries.AddRangeAsync(entries, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<DropListEntry>> GetByDropDateAsync(DateTime dropDate, CancellationToken cancellationToken = default)
    {
        return await _context.DropListEntries
            .Where(e => e.DropDate.Date == dropDate.Date)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<DropListEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DropListEntries.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string domainName, CancellationToken cancellationToken = default)
    {
        return await _context.DropListEntries
            .AnyAsync(e => e.DomainName == domainName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
