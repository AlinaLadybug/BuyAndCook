using System.Collections.Generic;

namespace BuyAndCook.Application.Models.Silpo
{
    public class SilpoCartAddress
    {
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string House { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Country { get; set; } = "Україна";
        public string? Locality { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? BranchIdWideassort { get; set; }
        public string? DeliveryProvider { get; set; }
    }

    public class SilpoCartTimeslot
    {
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
    }

    public class SilpoCartProductRequest
    {
        public string ProductId { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1;
        public string BranchId { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public IReadOnlyList<string> Modifications { get; set; } = new List<string>();
    }

    public class SilpoCartDraft
    {
        public SilpoCartAddress Address { get; set; } = new SilpoCartAddress();
        public string DeliveryType { get; set; } = "DeliveryHome";
        public string DeliveryProvider { get; set; } = "CityRider";
        public IReadOnlyList<SilpoCartShipment> Shipments { get; set; } = new List<SilpoCartShipment>();
        public SilpoCartTimeslot? Timeslot { get; set; }
    }

    public class SilpoCartUpdate
    {
        public SilpoCartAddress? Address { get; set; }
        public string? DeliveryType { get; set; }
        public string? DeliveryProvider { get; set; }
        public IReadOnlyList<SilpoCartShipment>? Shipments { get; set; }
        public SilpoCartTimeslot? Timeslot { get; set; }
    }

    public class SilpoCartShipment
    {
        public string Id { get; set; } = string.Empty;
        public string BranchId { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public IReadOnlyList<SilpoCartItem> Products { get; set; } = new List<SilpoCartItem>();
    }

    public class SilpoCartItem
    {
        public string ProductId { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public SilpoCartItemData Data { get; set; } = new SilpoCartItemData();
    }

    public class SilpoCartItemData
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string DisplayRatio { get; set; } = string.Empty;
        public int Stock { get; set; }

        public string? ImageUrl => string.IsNullOrWhiteSpace(Icon)
            ? null
            : $"https://images.silpo.ua/v2/products/400x400/webp/{Icon}";
    }

    public class SilpoCart
    {
        public string Id { get; set; } = string.Empty;
        public SilpoCartAddress Address { get; set; } = new SilpoCartAddress();
        public string DeliveryType { get; set; } = string.Empty;
        public string DeliveryProvider { get; set; } = string.Empty;
        public SilpoCartTimeslot? Timeslot { get; set; }
        public IReadOnlyList<SilpoCartShipment> Shipments { get; set; } = new List<SilpoCartShipment>();
    }
}
