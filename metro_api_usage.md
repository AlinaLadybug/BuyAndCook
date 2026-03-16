# Metro API Integration Notes

## Configuration
Web settings live in `/Users/ohaievskyi/RiderProjects/BuyAndCook/src/BuyAndCook.Web/appsettings.json` under `MetroApi`:
- `BaseUrl`: `https://shop.metro.ua`
- `Country`: `UA`
- `Locale`: `uk-UA`
- `DefaultStoreId`: default Metro store used for live anonymous carts
- `ShopUrl`: Metro web catalog root
- `BasketUrl`: Metro basket route used by the restore bookmarklet
- `PublicAnonymousCartEnabled`: documents whether Metro currently exposes a public anonymous-cart UI

Mobile uses the same default values from `MetroApiSettings`.

## Live API flow
BuyAndCook uses Metro's public storefront APIs to build a real anonymous cart:
1. `POST /ordercapture/anonymouscart/anon-carts`
2. `GET /searchdiscover/articlesearch/search`
3. `GET /evaluate.article.v1/betty-variants`
4. `POST /ordercapture/anonymouscart/anon-carts/{cartId}/items`
5. `GET /ordercapture/anonymouscart/anon-carts/{cartId}`

The app resolves recipe ingredients into Metro search terms, finds the first matching bundle, and adds one unit of each matched product.

## UI flow
1. Select recipes and click `Generate`.
2. Review `Send to Metro` and optionally use `Open Metro` or `Copy list` for manual search.
3. Click `Create Metro cart link`.
4. Open the generated internal cart page to verify which Metro products were added.
5. Use the Metro restore bookmarklet only for experimental handoff on `shop.metro.ua`.

## Cart links
- Internal cart viewer: `/metro/cart/{cartId}?storeId={storeId}&anonUserId={anonUserId}`
- Metro basket page: `https://shop.metro.ua/shop/anon-cart`
- Metro restore bookmarklet: generated `javascript:` payload that restores the anonymous cart state inside Metro's browser storage

## Metro Web Handoff
- BuyAndCook can create and read a real Metro anonymous cart.
- Metro's public UA storefront currently does not expose a customer-facing anonymous-cart confirmation flow.
- `window.ocCountryConfig.anonCart` resolves to `null` on `shop.metro.ua` for UA, so a Silpo-style one-click public cart confirmation link is not available.
- The restore bookmarklet writes the state Metro expects:
  - cookie `anonymousUserId`
  - cookie `SES2_customerAdr_`
  - `localStorage["anonymousCartId"]`
  - `localStorage["anonymousCartStore"]`
  - `localStorage["MShopCookieCategoriesC"]`
  - `localStorage["MShopCookieCategoriesL"]`
- Because Metro hides the public cart UI, the supported user path today is:
  - create the live Metro cart in BuyAndCook
  - verify it in the internal Metro cart viewer
  - continue with Metro search fallback on `shop.metro.ua`

## Testing
- `npx playwright test e2e/metro-cart-link.spec.js --workers=1`
- The Metro E2E test uses the live Metro APIs by default.
- The test verifies:
  - a real Metro anonymous cart is created
  - products are added to that cart
  - the internal Metro cart page can read the live cart back
  - the generated bookmarklet contains the required Metro cookies and localStorage keys

## Manual QA Checklist
- `Generate` creates a shopping list from selected recipes.
- `Send to Metro` shows Metro search terms and opens Metro search.
- `Create Metro cart link` creates a live Metro anonymous cart.
- The internal `/metro/cart/...` page shows the added Metro products.
- Copying the restore bookmarklet produces a `javascript:` string containing `anonymousCartId`, `anonymousCartStore`, `anonymousUserId`, and `SES2_customerAdr_`.
