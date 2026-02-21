using BuyAndCook.Domain.Models;

namespace BuyAndCook.Application.Abstractions
{
    public interface IGroceryProvider
    {
        string Id { get; }
        string DisplayName { get; }
        GroceryExport BuildExport(IEnumerable<Ingredient> items);
    }
}
