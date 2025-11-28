using DomainAgent.Configuration;
using DomainAgent.Models;
using Microsoft.Extensions.Options;

namespace DomainAgent.Services;

/// <summary>
/// Service for executing the domain purchase workflow.
/// </summary>
public class DomainPurchaseService : IDomainPurchaseService
{
    private readonly ITppWholesaleApiClient _apiClient;
    private readonly IDomainSelectionService _selectionService;
    private readonly DomainSelectionOptions _options;
    private readonly ILogger<DomainPurchaseService> _logger;

    public DomainPurchaseService(
        ITppWholesaleApiClient apiClient,
        IDomainSelectionService selectionService,
        IOptions<DomainSelectionOptions> options,
        ILogger<DomainPurchaseService> logger)
    {
        _apiClient = apiClient;
        _selectionService = selectionService;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<DomainOrderResponse>> ExecutePurchaseWorkflowAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<DomainOrderResponse>();

        try
        {
            _logger.LogInformation("Starting domain purchase workflow");

            // Step 1: Get the drop list
            var dropListResponse = await _apiClient.GetDropListAsync(cancellationToken);

            if (!dropListResponse.Success || dropListResponse.Data == null)
            {
                _logger.LogError("Failed to fetch drop list: {Error}", dropListResponse.ErrorMessage);
                return results;
            }

            _logger.LogInformation("Retrieved {Count} domains from drop list", dropListResponse.Data.Count);

            // Step 2: Select domains to buy
            var selectedDomains = _selectionService.SelectDomainsToBuy(dropListResponse.Data);

            if (selectedDomains.Count == 0)
            {
                _logger.LogInformation("No domains selected for purchase");
                return results;
            }

            _logger.LogInformation("Selected {Count} domains for purchase", selectedDomains.Count);

            // Step 3: Execute purchases
            foreach (var domain in selectedDomains)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Purchase workflow cancelled");
                    break;
                }

                try
                {
                    var orderRequest = CreateOrderRequest(domain);
                    var orderResponse = await _apiClient.OrderDomainAsync(orderRequest, cancellationToken);
                    results.Add(orderResponse);

                    if (orderResponse.Success)
                    {
                        _logger.LogInformation("Successfully ordered domain: {DomainName}", domain.DomainName);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to order domain: {DomainName}. Error: {Error}",
                            domain.DomainName, orderResponse.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ordering domain: {DomainName}", domain.DomainName);
                    results.Add(new DomainOrderResponse
                    {
                        Success = false,
                        DomainName = domain.DomainName,
                        ErrorMessage = ex.Message
                    });
                }
            }

            // Summary logging
            var successCount = results.Count(r => r.Success);
            var failCount = results.Count(r => !r.Success);
            _logger.LogInformation("Purchase workflow completed. Success: {SuccessCount}, Failed: {FailCount}",
                successCount, failCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during purchase workflow");
        }

        return results;
    }

    private DomainOrderRequest CreateOrderRequest(DropListDomain domain)
    {
        return new DomainOrderRequest
        {
            DomainName = domain.DomainName,
            Period = 1,
            RegistrantContactId = _options.DefaultRegistrantContactId,
            AdminContactId = _options.DefaultRegistrantContactId,
            TechContactId = _options.DefaultRegistrantContactId,
            BillingContactId = _options.DefaultRegistrantContactId,
            Nameservers = _options.DefaultNameservers.Count > 0 ? _options.DefaultNameservers : null
        };
    }
}
