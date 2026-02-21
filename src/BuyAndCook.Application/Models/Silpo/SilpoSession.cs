namespace BuyAndCook.Application.Models.Silpo
{
    public class SilpoSession
    {
        public SilpoUserLocation? Location { get; set; }
        public string? BranchId { get; set; }
        public string? DeliveryType { get; set; }
        public string? DeliveryProvider { get; set; }
        public string? CartId { get; set; }
    }
}
