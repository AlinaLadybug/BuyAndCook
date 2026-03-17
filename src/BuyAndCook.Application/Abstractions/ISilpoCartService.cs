using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuyAndCook.Application.Models.Silpo;

namespace BuyAndCook.Application.Abstractions
{
    public interface ISilpoCartService
    {
        Task<string> CreateCartAsync(SilpoCartDraft draft, CancellationToken cancellationToken = default);

        Task AddProductsAsync(
            string cartId,
            IReadOnlyList<SilpoCartProductRequest> products,
            CancellationToken cancellationToken = default);

        Task<SilpoCart?> GetCartAsync(string cartId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<SilpoDeliveryTimeSlot>> GetDeliveryTimeSlotsAsync(
            string branchId,
            IReadOnlyList<string> deliveryTypes,
            CancellationToken cancellationToken = default);

        Task UpdateCartAsync(
            string cartId,
            SilpoCartUpdate update,
            CancellationToken cancellationToken = default);
    }
}
