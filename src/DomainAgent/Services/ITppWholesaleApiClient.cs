using DomainAgent.Models;

namespace DomainAgent.Services;

/// <summary>
/// Interface for TPP Wholesale API client.
/// </summary>
public interface ITppWholesaleApiClient
{
    /// <summary>
    /// Gets the drop list of domains from the .au registry.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of domains in the drop list.</returns>
    Task<ApiResponse<List<DropListDomain>>> GetDropListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Places an order to register a domain.
    /// </summary>
    /// <param name="request">The domain order request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order response.</returns>
    Task<DomainOrderResponse> OrderDomainAsync(DomainOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a domain is available for registration.
    /// </summary>
    /// <param name="domainName">The domain name to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the domain is available.</returns>
    Task<bool> CheckDomainAvailabilityAsync(string domainName, CancellationToken cancellationToken = default);
}
