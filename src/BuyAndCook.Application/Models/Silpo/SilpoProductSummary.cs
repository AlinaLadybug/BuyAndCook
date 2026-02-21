namespace BuyAndCook.Application.Models.Silpo
{
    public class SilpoProductSummary
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? DisplayPrice { get; set; }
        public decimal? DisplayOldPrice { get; set; }
        public string DisplayRatio { get; set; } = string.Empty;
        public int Stock { get; set; }
        public string OfferId { get; set; } = string.Empty;
        public string BranchId { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string DeliveryType { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool Weighted { get; set; }
        public decimal AddToBasketStep { get; set; }
        public int ExternalProductId { get; set; }

        public string? ImageUrl => string.IsNullOrWhiteSpace(Icon)
            ? null
            : $"https://images.silpo.ua/v2/products/400x400/webp/{Icon}";
    }
}
