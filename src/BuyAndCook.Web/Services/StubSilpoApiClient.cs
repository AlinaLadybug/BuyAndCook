using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuyAndCook.Application.Abstractions;
using BuyAndCook.Application.Models.Silpo;

namespace BuyAndCook.Web.Services
{
    public class StubSilpoApiClient : ISilpoLocationService, ISilpoProductSearchService, ISilpoCartService
    {
        private readonly SilpoApiSettings _settings;
        private readonly Dictionary<string, SilpoCart> _carts = new();
        private readonly Dictionary<string, SilpoProductSummary> _products = new();
        private readonly object _sync = new();
        private int _cartCounter;
        private int _productCounter;

        public StubSilpoApiClient(SilpoApiSettings settings)
        {
            _settings = settings;
        }

        public Task<IReadOnlyList<SilpoAddressSuggestion>> SearchAddressesAsync(
            string query,
            double? latitude = null,
            double? longitude = null,
            CancellationToken cancellationToken = default)
        {
            var normalized = string.IsNullOrWhiteSpace(query) ? "Kyiv, Test Street" : query.Trim();
            var addressLine = normalized.Contains("10", StringComparison.OrdinalIgnoreCase)
                ? normalized
                : $"{normalized} 10";

            var suggestion = new SilpoAddressSuggestion
            {
                Address = addressLine,
                Street = "Test Street",
                HouseNumber = "10",
                City = "Kyiv",
                District = "Shevchenkivskyi",
                Latitude = 50.4501,
                Longitude = 30.5234,
                Type = "address"
            };

            return Task.FromResult<IReadOnlyList<SilpoAddressSuggestion>>(new List<SilpoAddressSuggestion> { suggestion });
        }

        public Task<SilpoBranchPolygon?> FindBranchByCoordinatesAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default)
        {
            var polygon = new SilpoBranchPolygon
            {
                BranchId = "stub-branch",
                BranchName = "Stub Branch",
                DeliveryType = _settings.DefaultDeliveryType
            };

            return Task.FromResult<SilpoBranchPolygon?>(polygon);
        }

        public Task<IReadOnlyList<SilpoBranch>> GetBranchesAsync(CancellationToken cancellationToken = default)
        {
            var branch = BuildBranch();
            return Task.FromResult<IReadOnlyList<SilpoBranch>>(new List<SilpoBranch> { branch });
        }

        public Task<SilpoBranch?> FindNearestBranchAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<SilpoBranch?>(BuildBranch());
        }

        public Task<SilpoProductSearchResult> QuickSearchAsync(
            string branchId,
            string query,
            int limit = 20,
            int offset = 0,
            string? sortBy = null,
            string? sortDirection = null,
            CancellationToken cancellationToken = default)
        {
            var trimmed = string.IsNullOrWhiteSpace(query) ? "Stub item" : query.Trim();
            var product = BuildProduct(branchId, trimmed);

            var result = new SilpoProductSearchResult
            {
                Limit = limit,
                Offset = offset,
                Total = 1,
                Items = new List<SilpoProductSummary> { product }
            };

            return Task.FromResult(result);
        }

        public Task<string> CreateCartAsync(SilpoCartDraft draft, CancellationToken cancellationToken = default)
        {
            var id = $"stub-cart-{Interlocked.Increment(ref _cartCounter)}";
            var shipments = draft.Shipments
                .Select(shipment => new SilpoCartShipment
                {
                    BranchId = shipment.BranchId,
                    CompanyId = shipment.CompanyId,
                    Products = new List<SilpoCartItem>()
                })
                .ToList();

            var cart = new SilpoCart
            {
                Id = id,
                Address = draft.Address,
                DeliveryType = draft.DeliveryType,
                DeliveryProvider = draft.DeliveryProvider,
                Shipments = shipments,
                Timeslot = draft.Timeslot
            };

            lock (_sync)
            {
                _carts[id] = cart;
            }

            return Task.FromResult(id);
        }

        public Task AddProductsAsync(
            string cartId,
            IReadOnlyList<SilpoCartProductRequest> products,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cartId))
            {
                return Task.CompletedTask;
            }

            lock (_sync)
            {
                if (!_carts.TryGetValue(cartId, out var cart))
                {
                    return Task.CompletedTask;
                }

                foreach (var request in products)
                {
                    var summary = ResolveProduct(request);
                    var price = summary.DisplayPrice ?? summary.Price ?? 10m;
                    var item = new SilpoCartItem
                    {
                        ProductId = summary.Id,
                        Quantity = request.Quantity,
                        Price = price,
                        Total = price * request.Quantity,
                        Data = new SilpoCartItemData
                        {
                            Title = summary.Title,
                            Icon = summary.Icon,
                            DisplayRatio = summary.DisplayRatio,
                            Stock = summary.Stock
                        }
                    };

                    var shipment = cart.Shipments.FirstOrDefault(s => s.BranchId == request.BranchId)
                                   ?? cart.Shipments.FirstOrDefault();
                    if (shipment == null)
                    {
                        continue;
                    }

                    if (shipment.Products is not List<SilpoCartItem> list)
                    {
                        list = shipment.Products.ToList();
                        shipment.Products = list;
                    }

                    var existing = list.FirstOrDefault(p => p.ProductId == item.ProductId);
                    if (existing != null)
                    {
                        existing.Quantity += item.Quantity;
                        existing.Total = existing.Quantity * existing.Price;
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task<SilpoCart?> GetCartAsync(string cartId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cartId))
            {
                return Task.FromResult<SilpoCart?>(null);
            }

            lock (_sync)
            {
                _carts.TryGetValue(cartId, out var cart);
                return Task.FromResult(cart);
            }
        }

        public Task<IReadOnlyList<SilpoDeliveryTimeSlot>> GetDeliveryTimeSlotsAsync(
            string branchId,
            IReadOnlyList<string> deliveryTypes,
            CancellationToken cancellationToken = default)
        {
            var slot = new SilpoDeliveryTimeSlot
            {
                Start = DateTime.UtcNow.AddHours(2).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                End = DateTime.UtcNow.AddHours(3).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                DeliveryType = deliveryTypes.FirstOrDefault() ?? _settings.DefaultDeliveryType
            };

            return Task.FromResult<IReadOnlyList<SilpoDeliveryTimeSlot>>(new List<SilpoDeliveryTimeSlot> { slot });
        }

        public Task UpdateCartAsync(string cartId, SilpoCartUpdate update, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cartId))
            {
                return Task.CompletedTask;
            }

            lock (_sync)
            {
                if (!_carts.TryGetValue(cartId, out var cart))
                {
                    return Task.CompletedTask;
                }

                if (update.Address != null)
                {
                    cart.Address = update.Address;
                }

                if (!string.IsNullOrWhiteSpace(update.DeliveryType))
                {
                    cart.DeliveryType = update.DeliveryType;
                }

                if (!string.IsNullOrWhiteSpace(update.DeliveryProvider))
                {
                    cart.DeliveryProvider = update.DeliveryProvider;
                }

                if (update.Shipments != null)
                {
                    cart.Shipments = update.Shipments;
                }

                if (update.Timeslot != null)
                {
                    cart.Timeslot = update.Timeslot;
                }
            }

            return Task.CompletedTask;
        }

        private SilpoBranch BuildBranch()
        {
            return new SilpoBranch
            {
                BranchId = "stub-branch",
                CompanyId = _settings.CompanyId,
                AddressFull = "Stub Branch",
                CityFull = "Kyiv",
                Latitude = 50.4501,
                Longitude = 30.5234,
                Open = true
            };
        }

        private SilpoProductSummary BuildProduct(string branchId, string title)
        {
            var id = $"stub-product-{Interlocked.Increment(ref _productCounter)}";
            var product = new SilpoProductSummary
            {
                Id = id,
                Title = title,
                Price = 10m,
                DisplayPrice = 10m,
                DisplayRatio = "pcs",
                Stock = 100,
                BranchId = branchId,
                CompanyId = _settings.CompanyId,
                Weighted = false,
                AddToBasketStep = 0.1m
            };

            _products[id] = product;
            return product;
        }

        private SilpoProductSummary ResolveProduct(SilpoCartProductRequest request)
        {
            if (_products.TryGetValue(request.ProductId, out var product))
            {
                return product;
            }

            return new SilpoProductSummary
            {
                Id = request.ProductId,
                Title = request.ProductId,
                Price = 10m,
                DisplayPrice = 10m,
                DisplayRatio = "pcs",
                Stock = 100,
                BranchId = request.BranchId,
                CompanyId = request.CompanyId,
                Weighted = false,
                AddToBasketStep = 0.1m
            };
        }
    }
}
