using DomainAgent.Configuration;
using DomainAgent.Models;
using Microsoft.Extensions.Options;

namespace DomainAgent.Services;

/// <summary>
/// Service for selecting domains to purchase from the drop list.
/// </summary>
public class DomainSelectionService : IDomainSelectionService
{
    private readonly DomainSelectionOptions _options;
    private readonly ILogger<DomainSelectionService> _logger;

    public DomainSelectionService(
        IOptions<DomainSelectionOptions> options,
        ILogger<DomainSelectionService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public List<DropListDomain> SelectDomainsToBuy(List<DropListDomain> dropListDomains)
    {
        _logger.LogInformation("Selecting domains from {Count} available domains", dropListDomains.Count);

        var selectedDomains = dropListDomains
            .Where(d => IsValidDomain(d))
            .OrderByDescending(d => CalculatePriority(d))
            .Take(_options.MaxDomainsPerDay)
            .ToList();

        _logger.LogInformation("Selected {Count} domains for purchase", selectedDomains.Count);

        return selectedDomains;
    }

    private bool IsValidDomain(DropListDomain domain)
    {
        // Check if domain has a valid name
        if (string.IsNullOrWhiteSpace(domain.DomainName))
        {
            return false;
        }

        // Extract the domain name without TLD for length check
        var domainParts = domain.DomainName.Split('.');
        if (domainParts.Length == 0)
        {
            return false;
        }

        var domainNamePart = domainParts[0];

        // Check domain length
        if (domainNamePart.Length < _options.MinDomainLength ||
            domainNamePart.Length > _options.MaxDomainLength)
        {
            return false;
        }

        // Check if TLD is allowed
        if (_options.AllowedTlds.Count > 0)
        {
            var tld = GetTld(domain.DomainName);
            if (!_options.AllowedTlds.Any(allowed =>
                tld.Equals(allowed, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        // Check for excluded keywords
        if (_options.ExcludeKeywords.Count > 0)
        {
            var lowerDomain = domain.DomainName.ToLowerInvariant();
            if (_options.ExcludeKeywords.Any(keyword =>
                lowerDomain.Contains(keyword.ToLowerInvariant())))
            {
                _logger.LogDebug("Domain {DomainName} excluded due to keyword filter", domain.DomainName);
                return false;
            }
        }

        return true;
    }

    private int CalculatePriority(DropListDomain domain)
    {
        var priority = 0;
        var lowerDomain = domain.DomainName.ToLowerInvariant();

        // Prioritize shorter domain names
        var domainParts = domain.DomainName.Split('.');
        if (domainParts.Length > 0)
        {
            priority += 100 - Math.Min(domainParts[0].Length, 100);
        }

        // Boost priority for domains containing priority keywords
        foreach (var keyword in _options.PriorityKeywords)
        {
            if (lowerDomain.Contains(keyword.ToLowerInvariant()))
            {
                priority += 50;
            }
        }

        // Prefer .au over .com.au, etc. (shorter TLDs)
        var tld = GetTld(domain.DomainName);
        if (tld.Equals(".au", StringComparison.OrdinalIgnoreCase))
        {
            priority += 20;
        }

        return priority;
    }

    private static string GetTld(string domainName)
    {
        var parts = domainName.Split('.');
        if (parts.Length >= 2)
        {
            // For domains like "example.com.au", return ".com.au"
            // For domains like "example.au", return ".au"
            var tldParts = parts.Skip(1);
            return "." + string.Join(".", tldParts);
        }

        return string.Empty;
    }
}
