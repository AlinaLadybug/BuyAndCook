namespace BuyAndCook.Application.Models.Metro
{
    public class MetroApiSettings
    {
        public string BaseUrl { get; set; } = "https://shop.metro.ua";
        public string Country { get; set; } = "UA";
        public string Locale { get; set; } = "uk-UA";
        public string DefaultStoreId { get; set; } = "00010";
        public string ShopUrl { get; set; } = "https://shop.metro.ua/shop";
        public string BasketUrl { get; set; } = "https://shop.metro.ua/shop/anon-cart";
        public bool PublicAnonymousCartEnabled { get; set; }
    }
}
