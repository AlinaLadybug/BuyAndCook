using System;

namespace BuyAndCook.Application.Models.Silpo
{
    public static class SilpoWebCartRestore
    {
        public static string BuildBookmarklet(string cartId, string branchId, string basketUrl)
        {
            if (string.IsNullOrWhiteSpace(cartId))
            {
                throw new ArgumentException("Cart id is required.", nameof(cartId));
            }

            if (string.IsNullOrWhiteSpace(branchId))
            {
                throw new ArgumentException("Branch id is required.", nameof(branchId));
            }

            var targetUrl = string.IsNullOrWhiteSpace(basketUrl)
                ? "https://silpo.ua/basket"
                : basketUrl.Trim();

            return "javascript:(()=>{"
                   + $"localStorage.setItem('basketId','{cartId}');"
                   + $"document.cookie='branchId={branchId}; path=/; SameSite=Lax';"
                   + "document.cookie='disable-ssr=yes; path=/; SameSite=Lax';"
                   + $"location.href='{targetUrl}';"
                   + "})();";
        }
    }
}
