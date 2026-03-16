using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuyAndCook.Application.Models.Metro;

namespace BuyAndCook.Application.Abstractions
{
    public interface IMetroCartService
    {
        Task<MetroCartBuildResult> BuildCartAsync(
            IReadOnlyList<string> searchTerms,
            CancellationToken cancellationToken = default);

        Task<MetroCart?> GetCartAsync(
            string cartId,
            string storeId,
            string anonUserId,
            CancellationToken cancellationToken = default);
    }
}
