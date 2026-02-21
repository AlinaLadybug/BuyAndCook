using BuyAndCook.Domain.Models;

namespace BuyAndCook.Application.Abstractions
{
    public interface IIngredientMappingRepository
    {
        Task SaveMappingAsync(IngredientMapping mapping);
        Task<IReadOnlyList<IngredientMapping>> GetMappingsAsync(string providerId);
    }
}
