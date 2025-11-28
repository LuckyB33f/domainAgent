namespace DomainAgent.Models;

/// <summary>
/// Represents a domain from the drop list.
/// </summary>
public class DropListDomain
{
    /// <summary>
    /// The domain name.
    /// </summary>
    public required string DomainName { get; set; }

    /// <summary>
    /// The date when the domain will be available.
    /// </summary>
    public DateTime DropDate { get; set; }

    /// <summary>
    /// The TLD of the domain (e.g., ".au", ".com.au").
    /// </summary>
    public string? Tld { get; set; }
}
