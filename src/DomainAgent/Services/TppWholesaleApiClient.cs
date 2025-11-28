using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DomainAgent.Configuration;
using DomainAgent.Models;
using Microsoft.Extensions.Options;

namespace DomainAgent.Services;

/// <summary>
/// Client for interacting with TPP Wholesale API.
/// </summary>
public class TppWholesaleApiClient : ITppWholesaleApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TppWholesaleOptions _options;
    private readonly ILogger<TppWholesaleApiClient> _logger;

    public TppWholesaleApiClient(
        HttpClient httpClient,
        IOptions<TppWholesaleOptions> options,
        ILogger<TppWholesaleApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        // Add authentication headers based on TPP Wholesale API requirements
        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _options.ApiKey);
        }

        if (!string.IsNullOrEmpty(_options.ApiSecret))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Api-Secret", _options.ApiSecret);
        }

        if (!string.IsNullOrEmpty(_options.ResellerId))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Reseller-Id", _options.ResellerId);
        }
    }

    /// <inheritdoc />
    public async Task<ApiResponse<List<DropListDomain>>> GetDropListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching .au drop list from TPP Wholesale API");

            // TPP Wholesale API endpoint for drop list (adjust based on actual API documentation)
            var response = await _httpClient.GetAsync("domains/droplist/au", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to fetch drop list. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                return new ApiResponse<List<DropListDomain>>
                {
                    Success = false,
                    ErrorMessage = $"API returned status code {response.StatusCode}: {errorContent}"
                };
            }

            var dropList = await response.Content.ReadFromJsonAsync<List<DropListDomain>>(cancellationToken);

            _logger.LogInformation("Successfully fetched {Count} domains from drop list", dropList?.Count ?? 0);

            return new ApiResponse<List<DropListDomain>>
            {
                Success = true,
                Data = dropList ?? []
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error while fetching drop list");
            return new ApiResponse<List<DropListDomain>>
            {
                Success = false,
                ErrorMessage = $"HTTP request failed: {ex.Message}"
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error while fetching drop list");
            return new ApiResponse<List<DropListDomain>>
            {
                Success = false,
                ErrorMessage = $"Failed to parse API response: {ex.Message}"
            };
        }
    }

    /// <inheritdoc />
    public async Task<DomainOrderResponse> OrderDomainAsync(DomainOrderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Placing order for domain: {DomainName}", request.DomainName);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            // TPP Wholesale API endpoint for domain registration (adjust based on actual API documentation)
            var response = await _httpClient.PostAsync("domains/register", jsonContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to order domain {DomainName}. Status: {StatusCode}, Error: {Error}",
                    request.DomainName, response.StatusCode, errorContent);

                return new DomainOrderResponse
                {
                    Success = false,
                    DomainName = request.DomainName,
                    ErrorMessage = $"API returned status code {response.StatusCode}: {errorContent}"
                };
            }

            var orderResponse = await response.Content.ReadFromJsonAsync<DomainOrderResponse>(cancellationToken);

            if (orderResponse != null)
            {
                orderResponse.Success = true;
                orderResponse.DomainName = request.DomainName;
                _logger.LogInformation("Successfully ordered domain: {DomainName}, OrderId: {OrderId}",
                    request.DomainName, orderResponse.OrderId);
            }

            return orderResponse ?? new DomainOrderResponse
            {
                Success = true,
                DomainName = request.DomainName
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error while ordering domain {DomainName}", request.DomainName);
            return new DomainOrderResponse
            {
                Success = false,
                DomainName = request.DomainName,
                ErrorMessage = $"HTTP request failed: {ex.Message}"
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON error while ordering domain {DomainName}", request.DomainName);
            return new DomainOrderResponse
            {
                Success = false,
                DomainName = request.DomainName,
                ErrorMessage = $"Failed to parse API response: {ex.Message}"
            };
        }
    }

    /// <inheritdoc />
    public async Task<bool> CheckDomainAvailabilityAsync(string domainName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking availability for domain: {DomainName}", domainName);

            // TPP Wholesale API endpoint for availability check (adjust based on actual API documentation)
            var response = await _httpClient.GetAsync($"domains/check/{Uri.EscapeDataString(domainName)}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Availability check failed for domain {DomainName}. Status: {StatusCode}",
                    domainName, response.StatusCode);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(cancellationToken);
            var isAvailable = ParseAvailabilityResponse(result);

            _logger.LogDebug("Domain {DomainName} availability: {IsAvailable}", domainName, isAvailable);

            return isAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability for domain {DomainName}", domainName);
            return false;
        }
    }

    private static bool ParseAvailabilityResponse(Dictionary<string, object>? result)
    {
        if (result == null || !result.TryGetValue("available", out var availableValue))
        {
            return false;
        }

        if (availableValue is bool boolValue)
        {
            return boolValue;
        }

        var stringValue = availableValue?.ToString();
        return stringValue?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
    }
}
