namespace DomainAgent.Data.Entities;

/// <summary>
/// Status of a purchase attempt.
/// </summary>
public enum PurchaseStatus
{
    /// <summary>
    /// Purchase is pending.
    /// </summary>
    Pending,

    /// <summary>
    /// Purchase was successful.
    /// </summary>
    Success,

    /// <summary>
    /// Purchase failed.
    /// </summary>
    Failed
}

/// <summary>
/// Represents a domain purchase record in the database.
/// </summary>
public class Purchase
{
    /// <summary>
    /// Unique identifier for the purchase record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The domain name that was purchased or attempted to purchase.
    /// </summary>
    public required string DomainName { get; set; }

    /// <summary>
    /// The TLD of the domain (e.g., ".au", ".com.au").
    /// </summary>
    public string? Tld { get; set; }

    /// <summary>
    /// The order ID from TPP Wholesale if successful.
    /// </summary>
    public string? OrderId { get; set; }

    /// <summary>
    /// Status of the purchase.
    /// </summary>
    public PurchaseStatus Status { get; set; }

    /// <summary>
    /// Error message if the purchase failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Date when the purchase was attempted.
    /// </summary>
    public DateTime PurchaseDate { get; set; }

    /// <summary>
    /// Date when the record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date when the record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
