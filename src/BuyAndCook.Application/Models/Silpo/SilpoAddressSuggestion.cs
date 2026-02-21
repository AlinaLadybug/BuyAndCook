namespace BuyAndCook.Application.Models.Silpo
{
    public class SilpoAddressSuggestion
    {
        public string Address { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string HouseNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
