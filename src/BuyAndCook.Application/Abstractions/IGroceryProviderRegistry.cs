namespace BuyAndCook.Application.Abstractions
{
    public interface IGroceryProviderRegistry
    {
        IReadOnlyList<IGroceryProvider> GetProviders();
        IGroceryProvider? GetProvider(string id);
    }
}
