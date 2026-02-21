using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BuyAndCook.Application.Abstractions;
using BuyAndCook.Application.Models.Silpo;

namespace BuyAndCook.Infrastructure.Integrations
{
    public class SilpoApiClient : ISilpoLocationService, ISilpoProductSearchService, ISilpoCartService
    {
        private readonly HttpClient _httpClient;
        private readonly SilpoApiSettings _settings;
        private readonly JsonSerializerOptions _jsonOptions;

        public SilpoApiClient(HttpClient httpClient, SilpoApiSettings settings)
        {
            _httpClient = httpClient;
            _settings = settings;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (!_httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://silpo.ua");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://silpo.ua/");
        }

        public async Task<IReadOnlyList<SilpoAddressSuggestion>> SearchAddressesAsync(
            string query,
            double? latitude = null,
            double? longitude = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<SilpoAddressSuggestion>();
            }

            var builder = new StringBuilder();
            builder.Append(_settings.ExternalBaseUrl.TrimEnd('/'));
            builder.Append("/v1/photon-search/addresses/search?address=");
            builder.Append(Uri.EscapeDataString(query));

            if (longitude.HasValue && latitude.HasValue)
            {
                builder.Append("&longitude=");
                builder.Append(longitude.Value.ToString(CultureInfo.InvariantCulture));
                builder.Append("&latitude=");
                builder.Append(latitude.Value.ToString(CultureInfo.InvariantCulture));
            }

            var response = await GetAsync<AddressSearchResponse>(builder.ToString(), cancellationToken);
            return response.Items
                .Select(item => new SilpoAddressSuggestion
                {
                    Address = item.Address ?? string.Empty,
                    Street = item.Street ?? string.Empty,
                    HouseNumber = item.HouseNumber ?? string.Empty,
                    City = item.City ?? string.Empty,
                    District = item.District ?? string.Empty,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude,
                    Type = item.Type ?? string.Empty
                })
                .ToList();
        }

        public async Task<SilpoBranchPolygon?> FindBranchByCoordinatesAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default)
        {
            var url = $"{_settings.EcomBaseUrl.TrimEnd('/')}/v2/polygons/contains" +
                      $"?latitude={latitude.ToString(CultureInfo.InvariantCulture)}" +
                      $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}" +
                      "&deliveryTypes[]=DeliveryHome&deliveryTypes[]=LongDelivery";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return ParsePolygon(document.RootElement);
        }

        public async Task<IReadOnlyList<SilpoBranch>> GetBranchesAsync(CancellationToken cancellationToken = default)
        {
            var url = $"{_settings.EcomBaseUrl.TrimEnd('/')}/v1/uk/branches";
            var response = await GetAsync<BranchesResponse>(url, cancellationToken);

            return response.Items
                .Select(item => new SilpoBranch
                {
                    BranchId = item.BranchId ?? string.Empty,
                    CompanyId = item.CompanyId ?? string.Empty,
                    ExternalId = item.ExternalId ?? string.Empty,
                    HasPickup = item.HasPickup,
                    Latitude = ParseDouble(item.Latitude),
                    Longitude = ParseDouble(item.Longitude),
                    CityFull = item.CityFull ?? string.Empty,
                    AddressFull = item.AddressFull ?? string.Empty,
                    Open = item.Open
                })
                .ToList();
        }

        public async Task<SilpoBranch?> FindNearestBranchAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default)
        {
            var branches = await GetBranchesAsync(cancellationToken);
            if (branches.Count == 0)
            {
                return null;
            }

            SilpoBranch? nearest = null;
            var nearestDistance = double.MaxValue;

            foreach (var branch in branches)
            {
                var distance = HaversineDistance(latitude, longitude, branch.Latitude, branch.Longitude);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = branch;
                }
            }

            return nearest;
        }

        public async Task<SilpoProductSearchResult> QuickSearchAsync(
            string branchId,
            string query,
            int limit = 20,
            int offset = 0,
            string? sortBy = null,
            string? sortDirection = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(branchId) || string.IsNullOrWhiteSpace(query))
            {
                return new SilpoProductSearchResult();
            }

            var url = new StringBuilder();
            url.Append(_settings.EcomBaseUrl.TrimEnd('/'));
            url.Append($"/v1/uk/branches/{branchId}/quick-search?search=");
            url.Append(Uri.EscapeDataString(query));
            url.Append($"&limit={limit}&offset={offset}");

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                url.Append("&sortBy=");
                url.Append(Uri.EscapeDataString(sortBy));
            }

            if (!string.IsNullOrWhiteSpace(sortDirection))
            {
                url.Append("&sortDirection=");
                url.Append(Uri.EscapeDataString(sortDirection));
            }

            var response = await GetAsync<QuickSearchResponse>(url.ToString(), cancellationToken);
            return new SilpoProductSearchResult
            {
                Limit = response.Limit,
                Offset = response.Offset,
                Total = response.Total,
                Items = response.Items
                    .Select(item => new SilpoProductSummary
                    {
                        Id = item.Id ?? string.Empty,
                        Title = item.Title ?? string.Empty,
                        Icon = item.Icon ?? string.Empty,
                        Price = item.Price,
                        OldPrice = item.OldPrice,
                        DisplayPrice = item.DisplayPrice,
                        DisplayOldPrice = item.DisplayOldPrice,
                        DisplayRatio = item.DisplayRatio ?? string.Empty,
                        Stock = item.Stock,
                        OfferId = item.OfferId ?? string.Empty,
                        BranchId = item.BranchId ?? string.Empty,
                        CompanyId = item.CompanyId ?? string.Empty,
                        DeliveryType = item.DeliveryType ?? string.Empty,
                        Slug = item.Slug ?? string.Empty,
                        Weighted = item.Weighted,
                        AddToBasketStep = item.AddToBasketStep,
                        ExternalProductId = item.ExternalProductId
                    })
                    .ToList()
            };
        }

        public async Task<string> CreateCartAsync(SilpoCartDraft draft, CancellationToken cancellationToken = default)
        {
            var url = $"{_settings.EcomBaseUrl.TrimEnd('/')}/v2/shopping-cart";
            var payload = new CartCreateRequest
            {
                Address = new CartCreateAddress
                {
                    City = draft.Address.City,
                    Street = draft.Address.Street,
                    House = draft.Address.House,
                    District = draft.Address.District,
                    Country = string.IsNullOrWhiteSpace(draft.Address.Country) ? "Україна" : draft.Address.Country,
                    Locality = draft.Address.Locality,
                    Latitude = draft.Address.Latitude.ToString(CultureInfo.InvariantCulture),
                    Longitude = draft.Address.Longitude.ToString(CultureInfo.InvariantCulture),
                    BranchIdWideassort = draft.Address.BranchIdWideassort ?? _settings.WideAssortBranchId,
                    DeliveryProvider = draft.Address.DeliveryProvider ?? draft.DeliveryProvider
                },
                DeliveryType = draft.DeliveryType,
                DeliveryProvider = draft.DeliveryProvider,
                Shipments = draft.Shipments.Select(shipment => new CartCreateShipment
                {
                    BranchId = shipment.BranchId,
                    CompanyId = shipment.CompanyId
                }).ToList(),
                Timeslot = draft.Timeslot == null ? null : new CartCreateTimeslot
                {
                    Start = draft.Timeslot.Start,
                    End = draft.Timeslot.End
                }
            };

            var response = await SendJsonAsync<CartCreateRequest, IdResponse>(HttpMethod.Post, url, payload, cancellationToken);
            return response.Id ?? string.Empty;
        }

        public async Task AddProductsAsync(
            string cartId,
            IReadOnlyList<SilpoCartProductRequest> products,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cartId) || products.Count == 0)
            {
                return;
            }

            var url = $"{_settings.EcomBaseUrl.TrimEnd('/')}/v2/shopping-cart/{cartId}/products";
            var payload = new CartAddProductsRequest
            {
                Products = products.Select(product => new CartAddProduct
                {
                    ProductId = product.ProductId,
                    Quantity = product.Quantity,
                    BranchId = product.BranchId,
                    CompanyId = product.CompanyId,
                    Modifications = product.Modifications.ToList()
                }).ToList()
            };

            await SendJsonAsync(HttpMethod.Post, url, payload, cancellationToken);
        }

        public async Task<SilpoCart?> GetCartAsync(string cartId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cartId))
            {
                return null;
            }

            var url = $"{_settings.EcomBaseUrl.TrimEnd('/')}/v2/uk/shopping-cart/{cartId}";
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var cart = await JsonSerializer.DeserializeAsync<CartResponse>(stream, _jsonOptions, cancellationToken);
            if (cart == null)
            {
                return null;
            }

            return new SilpoCart
            {
                Id = cart.Id ?? string.Empty,
                Address = cart.Address == null ? new SilpoCartAddress() : new SilpoCartAddress
                {
                    City = cart.Address.City ?? string.Empty,
                    Street = cart.Address.Street ?? string.Empty,
                    House = cart.Address.House ?? string.Empty,
                    District = cart.Address.District ?? string.Empty,
                    Country = cart.Address.Country ?? string.Empty,
                    Locality = cart.Address.Locality,
                    Latitude = ParseDouble(cart.Address.Latitude),
                    Longitude = ParseDouble(cart.Address.Longitude),
                    BranchIdWideassort = cart.Address.BranchIdWideassort,
                    DeliveryProvider = cart.Address.DeliveryProvider
                },
                DeliveryType = cart.DeliveryType ?? string.Empty,
                DeliveryProvider = cart.DeliveryProvider ?? string.Empty,
                Timeslot = cart.Timeslot == null ? null : new SilpoCartTimeslot
                {
                    Start = cart.Timeslot.Start ?? string.Empty,
                    End = cart.Timeslot.End ?? string.Empty
                },
                Shipments = cart.Shipments
                    .Select(shipment => new SilpoCartShipment
                    {
                        Id = shipment.Id ?? string.Empty,
                        BranchId = shipment.BranchId ?? string.Empty,
                        CompanyId = shipment.CompanyId ?? string.Empty,
                        Products = shipment.Products
                            .Select(product => new SilpoCartItem
                            {
                                ProductId = product.ProductId ?? string.Empty,
                                Slug = product.Slug ?? string.Empty,
                                Quantity = product.Quantity,
                                Price = product.Price,
                                Total = product.Total,
                                Data = product.Data == null ? new SilpoCartItemData() : new SilpoCartItemData
                                {
                                    Title = product.Data.Title ?? string.Empty,
                                    Icon = product.Data.Icon ?? string.Empty,
                                    DisplayRatio = product.Data.DisplayRatio ?? string.Empty,
                                    Stock = product.Data.Stock
                                }
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }

        public async Task UpdateCartAsync(
            string cartId,
            SilpoCartUpdate update,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cartId))
            {
                return;
            }

            var url = $"{_settings.EcomBaseUrl.TrimEnd('/')}/v2/shopping-cart/{cartId}";
            var payload = new CartUpdateRequest
            {
                Address = update.Address == null ? null : new CartCreateAddress
                {
                    City = update.Address.City,
                    Street = update.Address.Street,
                    House = update.Address.House,
                    District = update.Address.District,
                    Country = update.Address.Country,
                    Locality = update.Address.Locality,
                    Latitude = update.Address.Latitude.ToString(CultureInfo.InvariantCulture),
                    Longitude = update.Address.Longitude.ToString(CultureInfo.InvariantCulture),
                    BranchIdWideassort = update.Address.BranchIdWideassort,
                    DeliveryProvider = update.Address.DeliveryProvider
                },
                DeliveryType = update.DeliveryType,
                DeliveryProvider = update.DeliveryProvider,
                Shipments = update.Shipments?.Select(shipment => new CartCreateShipment
                {
                    BranchId = shipment.BranchId,
                    CompanyId = shipment.CompanyId
                }).ToList(),
                Timeslot = update.Timeslot == null ? null : new CartCreateTimeslot
                {
                    Start = update.Timeslot.Start,
                    End = update.Timeslot.End
                }
            };

            await SendJsonAsync(new HttpMethod("PATCH"), url, payload, cancellationToken);
        }

        private async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to deserialize response.");
            }

            return result;
        }

        private async Task SendJsonAsync<TPayload>(HttpMethod method, string url, TPayload payload, CancellationToken cancellationToken)
        {
            using var response = await SendJsonRequestAsync(method, url, payload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        private async Task<TResponse> SendJsonAsync<TPayload, TResponse>(
            HttpMethod method,
            string url,
            TPayload payload,
            CancellationToken cancellationToken)
        {
            using var response = await SendJsonRequestAsync(method, url, payload, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<TResponse>(stream, _jsonOptions, cancellationToken);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to deserialize response.");
            }

            return result;
        }

        private async Task<HttpResponseMessage> SendJsonRequestAsync<TPayload>(
            HttpMethod method,
            string url,
            TPayload payload,
            CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            using var request = new HttpRequestMessage(method, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            return await _httpClient.SendAsync(request, cancellationToken);
        }

        private static SilpoBranchPolygon? ParsePolygon(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("branchId", out var branchId))
                {
                    return new SilpoBranchPolygon
                    {
                        BranchId = branchId.GetString() ?? string.Empty,
                        BranchName = root.TryGetProperty("branchName", out var name) ? name.GetString() ?? string.Empty : string.Empty,
                        DeliveryType = root.TryGetProperty("deliveryType", out var deliveryType) ? deliveryType.GetString() ?? string.Empty : string.Empty
                    };
                }

                if (root.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    var first = items.EnumerateArray().FirstOrDefault();
                    if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("branchId", out var itemBranchId))
                    {
                        return new SilpoBranchPolygon
                        {
                            BranchId = itemBranchId.GetString() ?? string.Empty,
                            BranchName = first.TryGetProperty("branchName", out var itemName) ? itemName.GetString() ?? string.Empty : string.Empty,
                            DeliveryType = first.TryGetProperty("deliveryType", out var itemDelivery) ? itemDelivery.GetString() ?? string.Empty : string.Empty
                        };
                    }
                }
            }

            return null;
        }

        private static double ParseDouble(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0;
        }

        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double radius = 6371;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Pow(Math.Sin(dLon / 2), 2);
            var c = 2 * Math.Asin(Math.Sqrt(a));
            return radius * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private sealed class AddressSearchResponse
        {
            [JsonPropertyName("items")]
            public List<AddressSearchItem> Items { get; set; } = new();
        }

        private sealed class AddressSearchItem
        {
            [JsonPropertyName("address")]
            public string? Address { get; set; }

            [JsonPropertyName("street")]
            public string? Street { get; set; }

            [JsonPropertyName("houseNumber")]
            public string? HouseNumber { get; set; }

            [JsonPropertyName("city")]
            public string? City { get; set; }

            [JsonPropertyName("district")]
            public string? District { get; set; }

            [JsonPropertyName("latitude")]
            public double Latitude { get; set; }

            [JsonPropertyName("longitude")]
            public double Longitude { get; set; }

            [JsonPropertyName("type")]
            public string? Type { get; set; }
        }

        private sealed class BranchesResponse
        {
            [JsonPropertyName("items")]
            public List<BranchItem> Items { get; set; } = new();
        }

        private sealed class BranchItem
        {
            [JsonPropertyName("branchId")]
            public string? BranchId { get; set; }

            [JsonPropertyName("companyId")]
            public string? CompanyId { get; set; }

            [JsonPropertyName("externalId")]
            public string? ExternalId { get; set; }

            [JsonPropertyName("hasPickup")]
            public bool HasPickup { get; set; }

            [JsonPropertyName("latitude")]
            public string? Latitude { get; set; }

            [JsonPropertyName("longitude")]
            public string? Longitude { get; set; }

            [JsonPropertyName("cityFull")]
            public string? CityFull { get; set; }

            [JsonPropertyName("addressFull")]
            public string? AddressFull { get; set; }

            [JsonPropertyName("open")]
            public bool Open { get; set; }
        }

        private sealed class QuickSearchResponse
        {
            [JsonPropertyName("limit")]
            public int Limit { get; set; }

            [JsonPropertyName("offset")]
            public int Offset { get; set; }

            [JsonPropertyName("total")]
            public int Total { get; set; }

            [JsonPropertyName("items")]
            public List<QuickSearchItem> Items { get; set; } = new();
        }

        private sealed class QuickSearchItem
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("icon")]
            public string? Icon { get; set; }

            [JsonPropertyName("price")]
            public decimal? Price { get; set; }

            [JsonPropertyName("oldPrice")]
            public decimal? OldPrice { get; set; }

            [JsonPropertyName("displayPrice")]
            public decimal? DisplayPrice { get; set; }

            [JsonPropertyName("displayOldPrice")]
            public decimal? DisplayOldPrice { get; set; }

            [JsonPropertyName("displayRatio")]
            public string? DisplayRatio { get; set; }

            [JsonPropertyName("stock")]
            public int Stock { get; set; }

            [JsonPropertyName("offerId")]
            public string? OfferId { get; set; }

            [JsonPropertyName("branchId")]
            public string? BranchId { get; set; }

            [JsonPropertyName("companyId")]
            public string? CompanyId { get; set; }

            [JsonPropertyName("deliveryType")]
            public string? DeliveryType { get; set; }

            [JsonPropertyName("slug")]
            public string? Slug { get; set; }

            [JsonPropertyName("weighted")]
            public bool Weighted { get; set; }

            [JsonPropertyName("addToBasketStep")]
            public decimal AddToBasketStep { get; set; }

            [JsonPropertyName("externalProductId")]
            public int ExternalProductId { get; set; }
        }

        private sealed class CartCreateRequest
        {
            [JsonPropertyName("address")]
            public CartCreateAddress? Address { get; set; }

            [JsonPropertyName("deliveryType")]
            public string? DeliveryType { get; set; }

            [JsonPropertyName("deliveryProvider")]
            public string? DeliveryProvider { get; set; }

            [JsonPropertyName("shipments")]
            public List<CartCreateShipment> Shipments { get; set; } = new();

            [JsonPropertyName("timeslot")]
            public CartCreateTimeslot? Timeslot { get; set; }
        }

        private sealed class CartCreateAddress
        {
            [JsonPropertyName("city")]
            public string? City { get; set; }

            [JsonPropertyName("latitude")]
            public string? Latitude { get; set; }

            [JsonPropertyName("longitude")]
            public string? Longitude { get; set; }

            [JsonPropertyName("house")]
            public string? House { get; set; }

            [JsonPropertyName("country")]
            public string? Country { get; set; }

            [JsonPropertyName("district")]
            public string? District { get; set; }

            [JsonPropertyName("locality")]
            public string? Locality { get; set; }

            [JsonPropertyName("street")]
            public string? Street { get; set; }

            [JsonPropertyName("branchIdWideassort")]
            public string? BranchIdWideassort { get; set; }

            [JsonPropertyName("deliveryProvider")]
            public string? DeliveryProvider { get; set; }
        }

        private sealed class CartCreateShipment
        {
            [JsonPropertyName("companyId")]
            public string? CompanyId { get; set; }

            [JsonPropertyName("branchId")]
            public string? BranchId { get; set; }
        }

        private sealed class CartCreateTimeslot
        {
            [JsonPropertyName("start")]
            public string? Start { get; set; }

            [JsonPropertyName("end")]
            public string? End { get; set; }
        }

        private sealed class CartAddProductsRequest
        {
            [JsonPropertyName("products")]
            public List<CartAddProduct> Products { get; set; } = new();
        }

        private sealed class CartAddProduct
        {
            [JsonPropertyName("productId")]
            public string? ProductId { get; set; }

            [JsonPropertyName("quantity")]
            public decimal Quantity { get; set; }

            [JsonPropertyName("modifications")]
            public List<string> Modifications { get; set; } = new();

            [JsonPropertyName("branchId")]
            public string? BranchId { get; set; }

            [JsonPropertyName("companyId")]
            public string? CompanyId { get; set; }
        }

        private sealed class IdResponse
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }
        }

        private sealed class CartUpdateRequest
        {
            [JsonPropertyName("address")]
            public CartCreateAddress? Address { get; set; }

            [JsonPropertyName("deliveryType")]
            public string? DeliveryType { get; set; }

            [JsonPropertyName("deliveryProvider")]
            public string? DeliveryProvider { get; set; }

            [JsonPropertyName("shipments")]
            public List<CartCreateShipment>? Shipments { get; set; }

            [JsonPropertyName("timeslot")]
            public CartCreateTimeslot? Timeslot { get; set; }
        }

        private sealed class CartResponse
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("address")]
            public CartResponseAddress? Address { get; set; }

            [JsonPropertyName("deliveryType")]
            public string? DeliveryType { get; set; }

            [JsonPropertyName("deliveryProvider")]
            public string? DeliveryProvider { get; set; }

            [JsonPropertyName("timeslot")]
            public CartCreateTimeslot? Timeslot { get; set; }

            [JsonPropertyName("shipments")]
            public List<CartResponseShipment> Shipments { get; set; } = new();
        }

        private sealed class CartResponseAddress
        {
            [JsonPropertyName("city")]
            public string? City { get; set; }

            [JsonPropertyName("street")]
            public string? Street { get; set; }

            [JsonPropertyName("house")]
            public string? House { get; set; }

            [JsonPropertyName("district")]
            public string? District { get; set; }

            [JsonPropertyName("country")]
            public string? Country { get; set; }

            [JsonPropertyName("locality")]
            public string? Locality { get; set; }

            [JsonPropertyName("latitude")]
            public string? Latitude { get; set; }

            [JsonPropertyName("longitude")]
            public string? Longitude { get; set; }

            [JsonPropertyName("branchIdWideassort")]
            public string? BranchIdWideassort { get; set; }

            [JsonPropertyName("deliveryProvider")]
            public string? DeliveryProvider { get; set; }
        }

        private sealed class CartResponseShipment
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("branchId")]
            public string? BranchId { get; set; }

            [JsonPropertyName("companyId")]
            public string? CompanyId { get; set; }

            [JsonPropertyName("products")]
            public List<CartResponseProduct> Products { get; set; } = new();
        }

        private sealed class CartResponseProduct
        {
            [JsonPropertyName("productId")]
            public string? ProductId { get; set; }

            [JsonPropertyName("slug")]
            public string? Slug { get; set; }

            [JsonPropertyName("quantity")]
            public decimal Quantity { get; set; }

            [JsonPropertyName("price")]
            public decimal Price { get; set; }

            [JsonPropertyName("total")]
            public decimal Total { get; set; }

            [JsonPropertyName("data")]
            public CartResponseProductData? Data { get; set; }
        }

        private sealed class CartResponseProductData
        {
            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("icon")]
            public string? Icon { get; set; }

            [JsonPropertyName("displayRatio")]
            public string? DisplayRatio { get; set; }

            [JsonPropertyName("stock")]
            public int Stock { get; set; }
        }
    }
}
