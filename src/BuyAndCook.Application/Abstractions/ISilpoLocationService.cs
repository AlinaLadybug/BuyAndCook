using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuyAndCook.Application.Models.Silpo;

namespace BuyAndCook.Application.Abstractions
{
    public interface ISilpoLocationService
    {
        Task<IReadOnlyList<SilpoAddressSuggestion>> SearchAddressesAsync(
            string query,
            double? latitude = null,
            double? longitude = null,
            CancellationToken cancellationToken = default);

        Task<SilpoBranchPolygon?> FindBranchByCoordinatesAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<SilpoBranch>> GetBranchesAsync(CancellationToken cancellationToken = default);

        Task<SilpoBranch?> FindNearestBranchAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default);
    }
}
