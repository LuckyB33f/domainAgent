using DomainAgent.Models;

namespace DomainAgent.Services;

/// <summary>
/// Interface for domain purchase service.
/// </summary>
public interface IDomainPurchaseService
{
    /// <summary>
    /// Executes the domain purchase workflow.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of order responses for the purchased domains.</returns>
    Task<List<DomainOrderResponse>> ExecutePurchaseWorkflowAsync(CancellationToken cancellationToken = default);
}
