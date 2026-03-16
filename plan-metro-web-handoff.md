# Metro Web Handoff Plan

## Goal
- Add a Metro integration that can build a real cart from selected recipe ingredients and hand the user off toward Metro web checkout as far as the public Metro flow allows today.

## Current external constraints
- `shop.metro.ua` exposes working anonymous cart APIs:
  - `POST /ordercapture/anonymouscart/anon-carts`
  - `POST /ordercapture/anonymouscart/anon-carts/{cartId}/items`
  - `GET /ordercapture/anonymouscart/anon-carts/{cartId}`
- Product discovery is public:
  - `GET /searchdiscover/articlesearch/search`
  - `GET /evaluate.article.v1/betty-variants`
- Public Metro UA storefront currently disables anonymous-cart UI:
  - `window.ocCountryConfig.anonCart === null`
  - public search results show `Купуйте в ТЦ`, not a customer checkout cart
- Because of that, a Silpo-style one-click public Metro basket confirmation flow is not available on `shop.metro.ua` today.

## Delivery scope
1. Create a real Metro anonymous cart through live Metro APIs.
2. Resolve recipe ingredient search terms to Metro bundle IDs.
3. Add first matching Metro products into the cart.
4. Show a BuyAndCook internal Metro cart viewer with the created Metro cart contents.
5. Generate an experimental Metro restore bookmarklet for `shop.metro.ua`:
   - set `anonymousUserId` cookie
   - set `SES2_customerAdr_` cookie
   - set `anonymousCartId` and `anonymousCartStore` in localStorage
   - set `MShopCookieCategoriesC` and `MShopCookieCategoriesL`
6. Provide Metro search fallback links so the user can continue manually on Metro web while public anonymous-cart checkout remains disabled.
7. Cover the live Metro cart creation flow with E2E tests.

## Implementation steps
1. Add Metro application models and settings.
2. Implement a Metro API client for search, variant resolution, cart creation, cart loading, and add-to-cart.
3. Add Metro bookmarklet generation helper.
4. Add Metro cart UI to Web and Mobile pages.
5. Add a Metro cart viewer page in the web app.
6. Add live Metro E2E tests.
7. Document the Metro flow and the current Metro public-site limitation.

## Success criteria
- BuyAndCook can create a live Metro anonymous cart from recipe-derived ingredients.
- The created cart is visible in BuyAndCook via an internal Metro cart page.
- The Metro bookmarklet contains the required storage and cookie handoff state.
- Metro E2E verifies live cart creation and the generated handoff payload.
