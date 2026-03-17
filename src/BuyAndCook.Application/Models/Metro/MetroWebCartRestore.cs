using System;

namespace BuyAndCook.Application.Models.Metro
{
    public static class MetroWebCartRestore
    {
        public static string BuildBookmarklet(
            string cartId,
            string storeId,
            string anonUserId,
            string basketUrl)
        {
            if (string.IsNullOrWhiteSpace(cartId))
            {
                throw new ArgumentException("Cart id is required.", nameof(cartId));
            }

            if (string.IsNullOrWhiteSpace(storeId))
            {
                throw new ArgumentException("Store id is required.", nameof(storeId));
            }

            if (string.IsNullOrWhiteSpace(anonUserId))
            {
                throw new ArgumentException("Anonymous user id is required.", nameof(anonUserId));
            }

            var targetUrl = string.IsNullOrWhiteSpace(basketUrl)
                ? "https://shop.metro.ua/shop/anon-cart"
                : basketUrl.Trim();

            var cartIdValue = ToJsStringLiteral(cartId);
            var storeIdValue = ToJsStringLiteral(storeId);
            var anonUserIdValue = ToJsStringLiteral(anonUserId);
            var targetUrlValue = ToJsStringLiteral(targetUrl);
            var selectedAddressJson = "{\"addressId\":null,\"addressHash\":null,\"storeId\":" + storeIdValue + "}";
            var selectedAddressValue = ToJsStringLiteral(selectedAddressJson);

            return "javascript:(()=>{"
                   + "const cookieCategories=JSON.parse(localStorage.getItem('MShopCookieCategoriesC')||'{}');"
                   + "cookieCategories.anonymousUserId='necessary';"
                   + "cookieCategories['SES2_customerAdr_']='necessary';"
                   + "localStorage.setItem('MShopCookieCategoriesC',JSON.stringify(cookieCategories));"
                   + "const localCategories=JSON.parse(localStorage.getItem('MShopCookieCategoriesL')||'{}');"
                   + "localCategories.anonymousCartId='necessary';"
                   + "localCategories.anonymousCartStore='necessary';"
                   + "localStorage.setItem('MShopCookieCategoriesL',JSON.stringify(localCategories));"
                   + $"localStorage.setItem('anonymousCartId',{cartIdValue});"
                   + $"localStorage.setItem('anonymousCartStore',{storeIdValue});"
                   + "localStorage.setItem('anonymousCartId_timestamp',Date.now().toString());"
                   + "localStorage.setItem('anonymousCartStore_timestamp',Date.now().toString());"
                   + $"document.cookie='anonymousUserId='+encodeURIComponent({anonUserIdValue})+'; path=/; SameSite=Lax';"
                   + $"document.cookie='SES2_customerAdr_='+encodeURIComponent({selectedAddressValue})+'; path=/; SameSite=Lax';"
                   + $"location.href={targetUrlValue};"
                   + "})();";
        }

        private static string ToJsStringLiteral(string value)
        {
            return "\"" + value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n") + "\"";
        }
    }
}
