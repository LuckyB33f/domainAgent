namespace DomainAgent.Configuration;

/// <summary>
/// Configuration options for domain selection.
/// </summary>
public class DomainSelectionOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "DomainSelection";

    /// <summary>
    /// The TLDs to filter for in the drop list (e.g., ".au", ".com.au", ".net.au").
    /// </summary>
    public List<string> AllowedTlds { get; set; } = [".au"];

    /// <summary>
    /// Maximum number of domains to buy per day.
    /// </summary>
    public int MaxDomainsPerDay { get; set; } = 10;

    /// <summary>
    /// Minimum domain length to consider.
    /// </summary>
    public int MinDomainLength { get; set; } = 3;

    /// <summary>
    /// Maximum domain length to consider.
    /// </summary>
    public int MaxDomainLength { get; set; } = 63;

    /// <summary>
    /// Keywords to prioritize when selecting domains.
    /// </summary>
    public List<string> PriorityKeywords { get; set; } = [];

    /// <summary>
    /// Keywords to exclude from selection.
    /// </summary>
    public List<string> ExcludeKeywords { get; set; } = [];

    /// <summary>
    /// Default registrant contact ID for orders.
    /// </summary>
    public string? DefaultRegistrantContactId { get; set; }

    /// <summary>
    /// Default nameservers for orders.
    /// </summary>
    public List<string> DefaultNameservers { get; set; } = [];
}
