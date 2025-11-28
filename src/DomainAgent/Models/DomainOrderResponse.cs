namespace DomainAgent.Models;

/// <summary>
/// Represents the response from a domain order request.
/// </summary>
public class DomainOrderResponse
{
    /// <summary>
    /// Whether the order was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The order ID if successful.
    /// </summary>
    public string? OrderId { get; set; }

    /// <summary>
    /// The domain name that was ordered.
    /// </summary>
    public string? DomainName { get; set; }

    /// <summary>
    /// Error message if the order failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The status of the order.
    /// </summary>
    public string? Status { get; set; }
}
