using BuyAndCook.Application.Abstractions;

namespace BuyAndCook.Infrastructure.Integrations
{
    public class GroceryProviderRegistry : IGroceryProviderRegistry
    {
        private readonly IReadOnlyList<IGroceryProvider> _providers;

        public GroceryProviderRegistry(IEnumerable<IGroceryProvider> providers)
        {
            _providers = providers.ToList();
        }

        public IReadOnlyList<IGroceryProvider> GetProviders() => _providers;

        public IGroceryProvider? GetProvider(string id)
        {
            return _providers.FirstOrDefault(provider =>
                string.Equals(provider.Id, id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
