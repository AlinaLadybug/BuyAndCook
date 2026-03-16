using System;
using System.Collections.Generic;
using System.Linq;
using BuyAndCook.Application.Abstractions;
using BuyAndCook.Domain.Models;

namespace BuyAndCook.Infrastructure.Integrations
{
    public class MetroGroceryProvider : IGroceryProvider
    {
        public const string ProviderId = "metro";
        private const string CatalogUrl = "https://shop.metro.ua/shop";

        public string Id => ProviderId;
        public string DisplayName => "Metro";

        public GroceryExport BuildExport(IEnumerable<Ingredient> items)
        {
            var queries = items
                .Where(item => !string.IsNullOrWhiteSpace(item.Name))
                .Select(item => item.Name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new GroceryExport(CatalogUrl, queries);
        }
    }
}
