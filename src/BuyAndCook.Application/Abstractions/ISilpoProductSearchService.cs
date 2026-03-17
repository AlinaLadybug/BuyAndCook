using System.Threading;
using System.Threading.Tasks;
using BuyAndCook.Application.Models.Silpo;

namespace BuyAndCook.Application.Abstractions
{
    public interface ISilpoProductSearchService
    {
        Task<SilpoProductSearchResult> QuickSearchAsync(
            string branchId,
            string query,
            int limit = 20,
            int offset = 0,
            string? sortBy = null,
            string? sortDirection = null,
            CancellationToken cancellationToken = default);
    }
}
