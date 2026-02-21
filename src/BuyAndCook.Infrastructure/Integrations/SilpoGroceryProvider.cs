using BuyAndCook.Application.Abstractions;
using BuyAndCook.Domain.Models;

namespace BuyAndCook.Infrastructure.Integrations
{
    public class SilpoGroceryProvider : IGroceryProvider
    {
        public const string ProviderId = "silpo";
        private const string CatalogUrl = "https://silpo.ua/catalog";

        public string Id => ProviderId;
        public string DisplayName => "Silpo";

        public GroceryExport BuildExport(IEnumerable<Ingredient> items)
        {
            var queries = items
                .Where(item => !string.IsNullOrWhiteSpace(item.Name))
                .Select(item => item.Name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList();

            return new GroceryExport(CatalogUrl, queries);
        }
    }
}
