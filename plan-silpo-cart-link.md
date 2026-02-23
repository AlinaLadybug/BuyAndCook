# Plan - Silpo Cart Link From Recipes

## Goals
- Allow users to generate a Silpo cart link after selecting recipes.
- Automatically build a cart from the shopping list (using Silpo API search + add-to-cart).
- Provide a link to the Silpo basket and a shareable internal cart viewer.

## Non-goals
- Checkout and payment.
- Perfect product matching (use best-effort first search result).

## Steps
1. Add domain/UI flow for “Create Silpo cart link from recipes”.
   - Add a button in the Shopping List section.
   - Show progress, success link, and summary of skipped items.

2. Implement cart builder logic.
   - Read the shopping list, resolve search terms from mappings.
   - Ensure location/branch is selected (require Silpo session).
   - Create a new Silpo cart and batch add products.

3. Expose links.
   - Internal cart viewer `/silpo/cart/{cartId}`.
   - Silpo basket link from settings.

4. E2E tests.
   - Add Playwright test covering the presence and basic behavior of the new button and link UI.
   - Keep test deterministic (no live Silpo API dependency).

5. Verification and docs.
   - Update silpo_api_usage.md.
   - Run build/tests and capture screenshots for PR.
