using System.Collections.Generic;

namespace BuyAndCook.Application.Models.Metro
{
    public class MetroCartItem
    {
        public string BundleId { get; set; } = string.Empty;
        public string DisplayId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BundleSize { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class MetroCart
    {
        public string Id { get; set; } = string.Empty;
        public string StoreId { get; set; } = string.Empty;
        public string AnonUserId { get; set; } = string.Empty;
        public int CartVersion { get; set; }
        public IReadOnlyList<MetroCartItem> Items { get; set; } = new List<MetroCartItem>();
    }

    public class MetroCartBuildResult
    {
        public MetroCart? Cart { get; set; }
        public int AddedCount { get; set; }
        public IReadOnlyList<string> SkippedTerms { get; set; } = new List<string>();
    }
}
