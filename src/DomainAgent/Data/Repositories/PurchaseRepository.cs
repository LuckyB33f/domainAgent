using DomainAgent.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainAgent.Data.Repositories;

/// <summary>
/// Repository for purchase records.
/// </summary>
public class PurchaseRepository : IPurchaseRepository
{
    private readonly DomainAgentDbContext _context;

    public PurchaseRepository(DomainAgentDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(Purchase purchase, CancellationToken cancellationToken = default)
    {
        purchase.CreatedAt = DateTime.UtcNow;
        purchase.UpdatedAt = DateTime.UtcNow;
        await _context.Purchases.AddAsync(purchase, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Purchase purchase, CancellationToken cancellationToken = default)
    {
        purchase.UpdatedAt = DateTime.UtcNow;
        _context.Purchases.Update(purchase);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<Purchase?> GetByDomainNameAsync(string domainName, CancellationToken cancellationToken = default)
    {
        return await _context.Purchases
            .OrderByDescending(p => p.PurchaseDate)
            .FirstOrDefaultAsync(p => p.DomainName == domainName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<Purchase>> GetByStatusAsync(PurchaseStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Purchases
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Purchases
            .Where(p => p.PurchaseDate >= startDate && p.PurchaseDate <= endDate)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<Purchase>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Purchases
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
