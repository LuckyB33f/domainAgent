namespace DomainAgent.Models;

/// <summary>
/// Represents a request to order a domain.
/// </summary>
public class DomainOrderRequest
{
    /// <summary>
    /// The domain name to order.
    /// </summary>
    public required string DomainName { get; set; }

    /// <summary>
    /// The registration period in years.
    /// </summary>
    public int Period { get; set; } = 1;

    /// <summary>
    /// The registrant contact ID.
    /// </summary>
    public string? RegistrantContactId { get; set; }

    /// <summary>
    /// The admin contact ID.
    /// </summary>
    public string? AdminContactId { get; set; }

    /// <summary>
    /// The technical contact ID.
    /// </summary>
    public string? TechContactId { get; set; }

    /// <summary>
    /// The billing contact ID.
    /// </summary>
    public string? BillingContactId { get; set; }

    /// <summary>
    /// Nameservers to set for the domain.
    /// </summary>
    public List<string>? Nameservers { get; set; }
}
