# Plan - Silpo Web Handoff

## Goal
- Restore a BuyAndCook-generated Silpo cart inside the real `silpo.ua` basket.

## Findings
- `silpo.ua` restores the active basket from `localStorage["basketId"]`.
- The selected branch is also read from the `branchId` cookie.
- A normal HTTP link cannot write `silpo.ua` browser storage from BuyAndCook because of same-origin restrictions.
- The viable web handoff is a bookmarklet that runs on `silpo.ua`, writes `basketId` + `branchId`, then opens the basket page.

## Steps
1. Generate a real Silpo cart from recipes.
   - Keep the internal `/silpo/cart/{cartId}` viewer for sharing/debugging.
   - Preserve the created cart id and branch id.

2. Expose a Silpo web restore action.
   - Add a generated bookmarklet for the current cart.
   - Provide copy/open affordances and short user instructions.

3. Cover the handoff with live E2E.
   - Build the cart through BuyAndCook.
   - Verify the generated restore bookmarklet contains the real cart id and branch id.
   - Seed the same values into a real `silpo.ua` browser context and confirm the product appears in the basket.

4. Document the limitation and the working flow.
   - Explain why a plain `https://silpo.ua/...` deep link is not enough today.
