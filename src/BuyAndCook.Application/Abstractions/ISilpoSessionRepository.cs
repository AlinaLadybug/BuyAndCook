using System.Threading.Tasks;
using BuyAndCook.Application.Models.Silpo;

namespace BuyAndCook.Application.Abstractions
{
    public interface ISilpoSessionRepository
    {
        Task<SilpoSession?> GetSessionAsync();
        Task SaveSessionAsync(SilpoSession session);
        Task ClearSessionAsync();
    }
}
