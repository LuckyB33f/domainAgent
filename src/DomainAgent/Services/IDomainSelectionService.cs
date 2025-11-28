using DomainAgent.Models;

namespace DomainAgent.Services;

/// <summary>
/// Interface for domain selection service.
/// </summary>
public interface IDomainSelectionService
{
    /// <summary>
    /// Selects domains to buy from the drop list based on configured criteria.
    /// </summary>
    /// <param name="dropListDomains">The list of domains from the drop list.</param>
    /// <returns>A list of domains selected for purchase.</returns>
    List<DropListDomain> SelectDomainsToBuy(List<DropListDomain> dropListDomains);
}
