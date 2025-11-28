using DomainAgent.Services;
using Quartz;

namespace DomainAgent.Jobs;

/// <summary>
/// Quartz job for purchasing domains from the drop list.
/// Scheduled to run at 1:31 AM Australian Eastern time daily.
/// </summary>
[DisallowConcurrentExecution]
public class DomainPurchaseJob : IJob
{
    private readonly IDomainPurchaseService _purchaseService;
    private readonly ILogger<DomainPurchaseJob> _logger;

    public DomainPurchaseJob(
        IDomainPurchaseService purchaseService,
        ILogger<DomainPurchaseJob> logger)
    {
        _purchaseService = purchaseService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the domain purchase job.
    /// </summary>
    /// <param name="context">The job execution context.</param>
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Domain purchase job started at {Time}", DateTimeOffset.Now);

        try
        {
            var results = await _purchaseService.ExecutePurchaseWorkflowAsync(context.CancellationToken);

            var successCount = results.Count(r => r.Success);
            var failCount = results.Count(r => !r.Success);

            _logger.LogInformation(
                "Domain purchase job completed. Total: {Total}, Success: {Success}, Failed: {Failed}",
                results.Count, successCount, failCount);

            // Log successful purchases
            foreach (var result in results.Where(r => r.Success))
            {
                _logger.LogInformation("Purchased domain: {DomainName}, OrderId: {OrderId}",
                    result.DomainName, result.OrderId);
            }

            // Log failures
            foreach (var result in results.Where(r => !r.Success))
            {
                _logger.LogWarning("Failed to purchase domain: {DomainName}, Error: {Error}",
                    result.DomainName, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing domain purchase job");
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }
}
