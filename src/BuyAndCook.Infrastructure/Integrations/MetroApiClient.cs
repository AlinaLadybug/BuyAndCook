using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BuyAndCook.Application.Abstractions;
using BuyAndCook.Application.Models.Metro;

namespace BuyAndCook.Infrastructure.Integrations
{
    public class MetroApiClient : IMetroCartService
    {
        private readonly HttpClient _httpClient;
        private readonly MetroApiSettings _settings;

        public MetroApiClient(HttpClient httpClient, MetroApiSettings settings)
        {
            _httpClient = httpClient;
            _settings = settings;

            if (!_httpClient.DefaultRequestHeaders.Accept.Any(header => header.MediaType == "application/json"))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        public async Task<MetroCartBuildResult> BuildCartAsync(
            IReadOnlyList<string> searchTerms,
            CancellationToken cancellationToken = default)
        {
            var normalizedTerms = (searchTerms ?? Array.Empty<string>())
                .Where(term => !string.IsNullOrWhiteSpace(term))
                .Select(term => term.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalizedTerms.Count == 0)
            {
                return new MetroCartBuildResult();
            }

            var storeId = _settings.DefaultStoreId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(storeId))
            {
                throw new InvalidOperationException("Metro default store id is not configured.");
            }

            var anonUserId = Guid.NewGuid().ToString();
            var cartId = await CreateCartAsync(storeId, anonUserId, cancellationToken);

            var skippedTerms = new List<string>();
            var addedCount = 0;

            foreach (var term in normalizedTerms)
            {
                string? variantId = null;
                try
                {
                    variantId = await SearchFirstVariantIdAsync(term, storeId, cancellationToken);
                }
                catch
                {
                    skippedTerms.Add(term);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(variantId))
                {
                    skippedTerms.Add(term);
                    continue;
                }

                MetroBundleResolution? bundle = null;
                try
                {
                    bundle = await ResolveBundleAsync(variantId, storeId, cancellationToken);
                }
                catch
                {
                    skippedTerms.Add(term);
                    continue;
                }

                if (bundle == null || string.IsNullOrWhiteSpace(bundle.BundleId))
                {
                    skippedTerms.Add(term);
                    continue;
                }

                try
                {
                    await AddItemAsync(cartId, storeId, anonUserId, bundle.BundleId, 1m, cancellationToken);
                    addedCount++;
                }
                catch
                {
                    skippedTerms.Add(term);
                }
            }

            var cart = await GetCartAsync(cartId, storeId, anonUserId, cancellationToken)
                       ?? new MetroCart
                       {
                           Id = cartId,
                           StoreId = storeId,
                           AnonUserId = anonUserId
                       };

            return new MetroCartBuildResult
            {
                Cart = cart,
                AddedCount = addedCount,
                SkippedTerms = skippedTerms
            };
        }

        public async Task<MetroCart?> GetCartAsync(
            string cartId,
            string storeId,
            string anonUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cartId) ||
                string.IsNullOrWhiteSpace(storeId) ||
                string.IsNullOrWhiteSpace(anonUserId))
            {
                return null;
            }

            var url = BuildUrl(
                $"/ordercapture/anonymouscart/anon-carts/{Uri.EscapeDataString(cartId)}" +
                $"?country={Uri.EscapeDataString(_settings.Country)}" +
                $"&storeId={Uri.EscapeDataString(storeId)}" +
                $"&anonUserId={Uri.EscapeDataString(anonUserId)}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);

            using var document = await ParseJsonAsync(response, cancellationToken);
            if (!document.RootElement.TryGetProperty("data", out var dataElement) ||
                dataElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var items = new List<MetroCartItem>();
            if (dataElement.TryGetProperty("items", out var itemsElement) &&
                itemsElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in itemsElement.EnumerateObject())
                {
                    var itemElement = property.Value;
                    items.Add(new MetroCartItem
                    {
                        BundleId = GetString(itemElement, "bundleId") ?? property.Name,
                        DisplayId = GetString(itemElement, "displayId") ?? string.Empty,
                        Description = GetString(itemElement, "description") ?? string.Empty,
                        BundleSize = GetString(itemElement, "bundleSize") ?? string.Empty,
                        Quantity = GetDecimal(itemElement, "quantity"),
                        ImageUrl = GetImageUrl(itemElement)
                    });
                }
            }

            return new MetroCart
            {
                Id = GetString(dataElement, "cartId") ?? cartId,
                StoreId = storeId,
                AnonUserId = anonUserId,
                CartVersion = GetInt32(dataElement, "cartVersion"),
                Items = items
            };
        }

        private async Task<string> SearchFirstVariantIdAsync(
            string query,
            string storeId,
            CancellationToken cancellationToken)
        {
            var url = BuildUrl(
                "/searchdiscover/articlesearch/search" +
                $"?language={Uri.EscapeDataString(_settings.Locale)}" +
                $"&country={Uri.EscapeDataString(_settings.Country)}" +
                $"&storeId={Uri.EscapeDataString(storeId)}" +
                "&categories=false&facets=false&profile=searchSuggest&page=1&rows=3" +
                $"&query={Uri.EscapeDataString(query)}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);

            using var document = await ParseJsonAsync(response, cancellationToken);
            if (!document.RootElement.TryGetProperty("resultIds", out var resultIds) ||
                resultIds.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            return resultIds.EnumerateArray()
                .Select(item => item.GetString())
                .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item))
                ?? string.Empty;
        }

        private async Task<MetroBundleResolution?> ResolveBundleAsync(
            string variantId,
            string storeId,
            CancellationToken cancellationToken)
        {
            var url = BuildUrl(
                "/evaluate.article.v1/betty-variants" +
                $"?storeIds={Uri.EscapeDataString(storeId)}" +
                $"&ids={Uri.EscapeDataString(variantId)}" +
                $"&country={Uri.EscapeDataString(_settings.Country)}" +
                $"&locale={Uri.EscapeDataString(_settings.Locale)}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);

            using var document = await ParseJsonAsync(response, cancellationToken);
            if (!document.RootElement.TryGetProperty("result", out var resultElement) ||
                resultElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var articleProperty in resultElement.EnumerateObject())
            {
                if (!articleProperty.Value.TryGetProperty("variants", out var variantsElement) ||
                    variantsElement.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                foreach (var variantProperty in variantsElement.EnumerateObject())
                {
                    if (!variantProperty.Value.TryGetProperty("bettyVariantId", out var bettyVariantIdElement) ||
                        GetString(bettyVariantIdElement, "bettyVariantId") != variantId)
                    {
                        continue;
                    }

                    var description = GetString(variantProperty.Value, "description") ?? string.Empty;
                    if (!variantProperty.Value.TryGetProperty("bundles", out var bundlesElement) ||
                        bundlesElement.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    foreach (var bundleProperty in bundlesElement.EnumerateObject())
                    {
                        var bundleId = GetString(bundleProperty.Value, "bundleId", "bettyBundleId");
                        if (string.IsNullOrWhiteSpace(bundleId))
                        {
                            continue;
                        }

                        if (bundleProperty.Value.TryGetProperty("stores", out var storesElement) &&
                            storesElement.ValueKind == JsonValueKind.Object &&
                            storesElement.TryGetProperty(storeId, out _))
                        {
                            return new MetroBundleResolution(bundleId, description);
                        }

                        return new MetroBundleResolution(bundleId, description);
                    }
                }
            }

            return null;
        }

        private async Task<string> CreateCartAsync(
            string storeId,
            string anonUserId,
            CancellationToken cancellationToken)
        {
            var url = BuildUrl("/ordercapture/anonymouscart/anon-carts");
            var payload = JsonSerializer.Serialize(new
            {
                requestId = Guid.NewGuid().ToString(),
                country = _settings.Country,
                storeId,
                anonUserId
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);

            using var document = await ParseJsonAsync(response, cancellationToken);
            return document.RootElement.TryGetProperty("data", out var dataElement)
                ? dataElement.GetString() ?? string.Empty
                : string.Empty;
        }

        private async Task AddItemAsync(
            string cartId,
            string storeId,
            string anonUserId,
            string bundleId,
            decimal quantity,
            CancellationToken cancellationToken)
        {
            var url = BuildUrl(
                $"/ordercapture/anonymouscart/anon-carts/{Uri.EscapeDataString(cartId)}/items" +
                $"?country={Uri.EscapeDataString(_settings.Country)}" +
                $"&storeId={Uri.EscapeDataString(storeId)}" +
                $"&anonUserId={Uri.EscapeDataString(anonUserId)}");

            var payload = JsonSerializer.Serialize(new
            {
                requestId = Guid.NewGuid().ToString(),
                bundleId,
                quantity
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        private string BuildUrl(string pathAndQuery)
        {
            return $"{_settings.BaseUrl.TrimEnd('/')}{pathAndQuery}";
        }

        private static async Task<JsonDocument> ParseJsonAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            await using var stream = await response.Content.ReadAsStreamAsync();
            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }

        private static async Task EnsureSuccessAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Metro API request failed with {(int)response.StatusCode}: {body}");
        }

        private static string? GetString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var property)
                ? property.GetString()
                : null;
        }

        private static string? GetString(JsonElement element, string propertyName, string nestedPropertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property) ||
                property.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            return GetString(property, nestedPropertyName);
        }

        private static int GetInt32(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                return 0;
            }

            return property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var result)
                ? result
                : 0;
        }

        private static decimal GetDecimal(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                return 0m;
            }

            switch (property.ValueKind)
            {
                case JsonValueKind.Number when property.TryGetDecimal(out var decimalValue):
                    return decimalValue;
                case JsonValueKind.String when decimal.TryParse(
                    property.GetString(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var parsed):
                    return parsed;
                default:
                    return 0m;
            }
        }

        private static string? GetImageUrl(JsonElement itemElement)
        {
            if (!itemElement.TryGetProperty("imageSources", out var imageSources) ||
                imageSources.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            return GetString(imageSources, "imageUrl1")
                   ?? GetString(imageSources, "imageUrl2")
                   ?? GetString(imageSources, "imageUrl3")
                   ?? GetString(imageSources, "imageUrlS");
        }

        private sealed class MetroBundleResolution
        {
            public MetroBundleResolution(string bundleId, string description)
            {
                BundleId = bundleId;
                Description = description;
            }

            public string BundleId { get; }
            public string Description { get; }
        }
    }
}
