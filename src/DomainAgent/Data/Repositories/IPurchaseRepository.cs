using DomainAgent.Data.Entities;

namespace DomainAgent.Data.Repositories;

/// <summary>
/// Interface for purchase repository.
/// </summary>
public interface IPurchaseRepository
{
    /// <summary>
    /// Adds a new purchase record.
    /// </summary>
    /// <param name="purchase">The purchase to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(Purchase purchase, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing purchase record.
    /// </summary>
    /// <param name="purchase">The purchase to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(Purchase purchase, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a purchase by domain name.
    /// </summary>
    /// <param name="domainName">The domain name to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The purchase record, or null if not found.</returns>
    Task<Purchase?> GetByDomainNameAsync(string domainName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all purchases with a specific status.
    /// </summary>
    /// <param name="status">The status to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of purchases.</returns>
    Task<List<Purchase>> GetByStatusAsync(PurchaseStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all purchases within a date range.
    /// </summary>
    /// <param name="startDate">Start date.</param>
    /// <param name="endDate">End date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of purchases.</returns>
    Task<List<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all purchases.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all purchases.</returns>
    Task<List<Purchase>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
