namespace BuyAndCook.Application.Models.Silpo
{
    public class SilpoBranch
    {
        public string BranchId { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public bool HasPickup { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CityFull { get; set; } = string.Empty;
        public string AddressFull { get; set; } = string.Empty;
        public bool Open { get; set; }
    }
}
