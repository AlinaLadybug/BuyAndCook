namespace BuyAndCook.Application.Models.Silpo
{
    public class SilpoApiSettings
    {
        public string EcomBaseUrl { get; set; } = "https://sf-ecom-api.silpo.ua";
        public string ExternalBaseUrl { get; set; } = "https://sf-external-api.silpo.ua";
        public string DefaultBranchId { get; set; } = "1edb6b58-cf2f-6c14-b8ca-d11f2666a570";
        public string CompanyId { get; set; } = "1ec88c5d-a050-669c-8467-570a157f3e31";
        public string DefaultDeliveryType { get; set; } = "DeliveryHome";
        public string DefaultDeliveryProvider { get; set; } = "CityRider";
        public string? WideAssortBranchId { get; set; } = "1f05d7b8-27b0-6762-8aea-896c4e98f56d";
    }
}
