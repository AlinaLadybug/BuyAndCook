# Silpo API Integration Notes

## Configuration
Web settings live in `src/BuyAndCook.Web/appsettings.json` under `SilpoApi`:
- `EcomBaseUrl`: `https://sf-ecom-api.silpo.ua`
- `ExternalBaseUrl`: `https://sf-external-api.silpo.ua`
- `DefaultBranchId`: default branch for search without location
- `CompanyId`: Silpo company ID
- `DefaultDeliveryType`: defaults to `DeliveryHome`
- `DefaultDeliveryProvider`: defaults to `CityRider`
- `WideAssortBranchId`: optional wide assortment branch
- `BasketUrl`: Silpo basket URL

Mobile uses the same defaults from `SilpoApiSettings`.

## UI Flow
1. Enter an address and search.
2. Pick an address suggestion to resolve the nearest branch.
3. Create a cart for the selected branch.
4. Search products and add them to the cart.
5. Refresh the cart to confirm contents.
6. Use the “Cart link” buttons to open/copy the shareable cart view (Web only).

## Recipe cart link flow
1. Select recipes and generate a shopping list.
2. Click “Create Silpo cart link”.
3. If location is missing, select an address first.
4. Drag the generated “Silpo restore bookmarklet” to the browser bookmarks bar.
5. Open `https://silpo.ua/basket`.
6. Run the saved bookmarklet to load the exact basket.

## Cart links
- Internal cart viewer: `/silpo/cart/{cartId}` (Web) for sharing a read-only cart view.
- Silpo basket: `https://silpo.ua/basket`.
- Silpo web restore: generated bookmarklet that writes `localStorage["basketId"]` and `branchId`, then opens the basket page.

## Silpo Web Handoff
- Silpo’s web cart is restored from browser storage, not from a normal public URL.
- The key used by the site is `localStorage["basketId"]`; the active branch is also read from the `branchId` cookie.
- Because browsers block cross-domain writes to `silpo.ua` storage, a regular HTTP link from BuyAndCook cannot prefill the Silpo basket directly.
- The implemented handoff is a bookmarklet: save it from BuyAndCook, open `silpo.ua`, then run it to restore the generated cart in the real Silpo basket.

## Testing
- Playwright runs against the real Silpo endpoints by default.
- Set `SILPO_API_STUB=true` to use the local stub (deterministic tests).
- Optional: `SILPO_E2E_ADDRESS="Kyiv, Khreshchatyk 10"` to override the live test address.
- Ensure the address includes a house number; the E2E flow selects the first suggestion containing digits.
- The live basket handoff check launches an additional headed Chromium window for `silpo.ua`.
- The live handoff test validates both parts:
  - BuyAndCook generates a real Silpo cart and a restore bookmarklet.
  - Playwright seeds the same `basketId` + `branchId` into a real `silpo.ua` browser context and verifies the product appears on the live basket page.
- E2E uses an isolated SQLite file under the OS temp directory by default (see `BUYANDCOOK_DB_PATH` in `playwright.config.js`).
- Set `PLAYWRIGHT_DB_ISOLATED=false` or `PLAYWRIGHT_REUSE_SERVER=true` if you want to reuse a running web server.

## Manual QA Checklist
- Address search returns suggestions.
- Selecting a suggestion resolves a branch and persists session.
- Cart creation succeeds for selected address/branch.
- Product search returns results for the resolved branch.
- Add-to-cart updates the cart and totals.
- Refresh cart pulls the latest cart state.
- Clear cart removes stored cart ID but keeps location.
- “Create Silpo cart link” builds a cart from the shopping list (or shows a location warning).
- Copying the restore bookmarklet produces a `javascript:` string containing the generated cart ID and branch ID.
