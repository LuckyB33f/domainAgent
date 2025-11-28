namespace DomainAgent.Configuration;

/// <summary>
/// Configuration options for TPP Wholesale API.
/// </summary>
public class TppWholesaleOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "TppWholesale";

    /// <summary>
    /// The base URL for the TPP Wholesale API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://www.tppwholesale.com.au/api/";

    /// <summary>
    /// The API key for authentication.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// The API secret for authentication.
    /// </summary>
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// The reseller ID.
    /// </summary>
    public string ResellerId { get; set; } = string.Empty;
}
