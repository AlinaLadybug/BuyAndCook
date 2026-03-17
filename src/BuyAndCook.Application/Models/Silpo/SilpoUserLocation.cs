namespace BuyAndCook.Application.Models.Silpo
{
    public class SilpoUserLocation
    {
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string House { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Country { get; set; } = "Україна";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
