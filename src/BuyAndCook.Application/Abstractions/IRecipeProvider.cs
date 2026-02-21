using BuyAndCook.Domain.Models;

namespace BuyAndCook.Application.Abstractions
{
    public interface IRecipeProvider
    {
        Task<IReadOnlyList<Recipe>> GetRecipesAsync();
    }
}
