using DomainAgent.Data.Entities;

namespace DomainAgent.Data.Repositories;

/// <summary>
/// Interface for drop list entry repository.
/// </summary>
public interface IDropListRepository
{
    /// <summary>
    /// Adds a new drop list entry.
    /// </summary>
    /// <param name="entry">The entry to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(DropListEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple drop list entries.
    /// </summary>
    /// <param name="entries">The entries to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddRangeAsync(IEnumerable<DropListEntry> entries, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all drop list entries for a specific drop date.
    /// </summary>
    /// <param name="dropDate">The drop date to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of drop list entries.</returns>
    Task<List<DropListEntry>> GetByDropDateAsync(DateTime dropDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all drop list entries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all drop list entries.</returns>
    Task<List<DropListEntry>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a domain exists in the drop list.
    /// </summary>
    /// <param name="domainName">The domain name to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the domain exists in the drop list.</returns>
    Task<bool> ExistsAsync(string domainName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
