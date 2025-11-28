namespace DomainAgent.Models;

/// <summary>
/// Generic API response wrapper.
/// </summary>
/// <typeparam name="T">The type of data returned.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Whether the API call was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The response data.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error message if the call failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Error code if the call failed.
    /// </summary>
    public string? ErrorCode { get; set; }
}
