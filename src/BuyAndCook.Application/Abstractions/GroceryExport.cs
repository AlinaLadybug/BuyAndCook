namespace BuyAndCook.Application.Abstractions
{
    public class GroceryExport
    {
        public string CatalogUrl { get; }
        public IReadOnlyList<string> SearchQueries { get; }

        public GroceryExport(string catalogUrl, IReadOnlyList<string> searchQueries)
        {
            CatalogUrl = catalogUrl;
            SearchQueries = searchQueries;
        }
    }
}
