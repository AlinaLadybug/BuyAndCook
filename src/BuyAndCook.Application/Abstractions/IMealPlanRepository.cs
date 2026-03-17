using BuyAndCook.Domain.Models;

namespace BuyAndCook.Application.Abstractions
{
    public interface IMealPlanRepository
    {
        Task<IReadOnlyList<MealPlanItem>> GetMealPlanAsync();
        Task SaveMealPlanAsync(IEnumerable<MealPlanItem> items);
    }
}
