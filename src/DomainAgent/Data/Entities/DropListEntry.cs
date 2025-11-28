namespace DomainAgent.Data.Entities;

/// <summary>
/// Represents a domain from the drop list stored in the database.
/// </summary>
public class DropListEntry
{
    /// <summary>
    /// Unique identifier for the drop list entry.
    /// </summary>
    public int Id { get; set; }

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

    /// <summary>
    /// Source of the drop list entry (e.g., "API", "CSV").
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Date when the entry was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date when the entry was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
