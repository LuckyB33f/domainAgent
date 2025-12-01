using DomainAgent.Configuration;
using DomainAgent.Data.Entities;
using DomainAgent.Data.Repositories;
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
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IDropListRepository _dropListRepository;
    private readonly DomainSelectionOptions _options;
    private readonly ILogger<DomainPurchaseService> _logger;

    public DomainPurchaseService(
        ITppWholesaleApiClient apiClient,
        IDomainSelectionService selectionService,
        IPurchaseRepository purchaseRepository,
        IDropListRepository dropListRepository,
        IOptions<DomainSelectionOptions> options,
        ILogger<DomainPurchaseService> logger)
    {
        _apiClient = apiClient;
        _selectionService = selectionService;
        _purchaseRepository = purchaseRepository;
        _dropListRepository = dropListRepository;
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

            // Step 1.5: Save the drop list to database
            await SaveDropListToDatabase(dropListResponse.Data, cancellationToken);

            // Step 2: Select domains to buy
            var selectedDomains = _selectionService.SelectDomainsToBuy(dropListResponse.Data);

            if (selectedDomains.Count == 0)
            {
                _logger.LogInformation("No domains selected for purchase");
                return results;
            }

            _logger.LogInformation("Selected {Count} domains for purchase", selectedDomains.Count);

            // Step 3: Execute purchases and save to database
            foreach (var domain in selectedDomains)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Purchase workflow cancelled");
                    break;
                }

                // Create pending purchase record
                var purchase = new Purchase
                {
                    DomainName = domain.DomainName,
                    Tld = domain.Tld,
                    Status = PurchaseStatus.Pending,
                    PurchaseDate = DateTime.UtcNow
                };

                try
                {
                    // Save pending purchase
                    await _purchaseRepository.AddAsync(purchase, cancellationToken);
                    await _purchaseRepository.SaveChangesAsync(cancellationToken);

                    var orderRequest = CreateOrderRequest(domain);
                    var orderResponse = await _apiClient.OrderDomainAsync(orderRequest, cancellationToken);
                    results.Add(orderResponse);

                    // Update purchase status based on result
                    if (orderResponse.Success)
                    {
                        purchase.Status = PurchaseStatus.Success;
                        purchase.OrderId = orderResponse.OrderId;
                        _logger.LogInformation("Successfully ordered domain: {DomainName}", domain.DomainName);
                    }
                    else
                    {
                        purchase.Status = PurchaseStatus.Failed;
                        purchase.ErrorMessage = orderResponse.ErrorMessage;
                        _logger.LogWarning("Failed to order domain: {DomainName}. Error: {Error}",
                            domain.DomainName, orderResponse.ErrorMessage);
                    }

                    // Save updated purchase status
                    await _purchaseRepository.UpdateAsync(purchase, cancellationToken);
                    await _purchaseRepository.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ordering domain: {DomainName}", domain.DomainName);

                    // Update purchase as failed
                    purchase.Status = PurchaseStatus.Failed;
                    purchase.ErrorMessage = ex.Message;
                    await _purchaseRepository.UpdateAsync(purchase, cancellationToken);
                    await _purchaseRepository.SaveChangesAsync(cancellationToken);

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

    private async Task SaveDropListToDatabase(List<DropListDomain> dropList, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Saving {Count} drop list entries to database", dropList.Count);

        var entriesToAdd = new List<DropListEntry>();

        foreach (var domain in dropList)
        {
            var normalizedDomainName = domain.DomainName.ToLowerInvariant();
            // Skip duplicates
            if (!await _dropListRepository.ExistsAsync(normalizedDomainName, cancellationToken))
            {
                entriesToAdd.Add(new DropListEntry
                {
                    DomainName = normalizedDomainName,
                    DropDate = domain.DropDate,
                    Tld = domain.Tld,
                    Source = "API"
                });
            }
        }

        if (entriesToAdd.Count > 0)
        {
            await _dropListRepository.AddRangeAsync(entriesToAdd, cancellationToken);
            await _dropListRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Saved {Count} new drop list entries to database", entriesToAdd.Count);
        }
        else
        {
            _logger.LogInformation("No new drop list entries to save");
        }
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
