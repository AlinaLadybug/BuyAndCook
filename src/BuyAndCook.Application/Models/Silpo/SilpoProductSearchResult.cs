using System.Collections.Generic;

namespace BuyAndCook.Application.Models.Silpo
{
    public class SilpoProductSearchResult
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Total { get; set; }
        public IReadOnlyList<SilpoProductSummary> Items { get; set; } = new List<SilpoProductSummary>();
    }
}
