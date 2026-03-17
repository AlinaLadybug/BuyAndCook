# MVP Plan — BuyAndCook

**Goals**
- Deliver MVP with Android, iOS, and Web clients.
- Show a small curated set of recipes sourced from Cookpad (menu list) via JSON dataset.
- Support meal planning (menu list), shopping list generation, and “Send to Silpo”.
- Keep grocery integrations modular to add more providers later.

**Scope (MVP)**
- Recipe list (Cookpad-based JSON), recipe detail, simple favorites.
- Meal planning list (weekly list, no calendar sync yet).
- Shopping list aggregation from planned recipes.
- Silpo integration via external store flow (see below).
- No accounts, no payments, no nutrition tracking.

**Architecture**
- Split solution into shared core and platform clients.
- Projects:
- BuyAndCook.Domain (entities, value objects).
- BuyAndCook.Application (use cases, interfaces).
- BuyAndCook.Infrastructure (data storage, recipe source adapters).
- BuyAndCook.Mobile (MAUI XAML app for Android/iOS).
- BuyAndCook.Web (ASP.NET Core Blazor Web App).
- Integration layer:
- IGroceryProvider
- IProductSearch
- ICartBridge
- Provider registry + feature flags

**Recipe Source (Cookpad)**
- Start with a small curated dataset (manual capture) to avoid brittle scraping.
- Store only required fields: title, image, ingredients, steps, source link.
- Add attribution and “Open original recipe” link.
- Prepare an adapter interface so future sources can be added.
- Dataset format/location:
- JSON file at `Resources/Raw/recipes.json`.
- Fields: `id`, `name`, `imageUrl`, `sourceUrl`, `ingredients` (`name`, `quantity`, `unit`), `steps`.
- Web uses the same dataset via content link to `wwwroot/data/recipes.json`.

**Selected Cookpad Recipes (MVP batch)**
- Borscht — https://cookpad.com/ua/recipes/13866129
- Plov — https://cookpad.com/ua/recipes/14521456
- Syrnyky — https://cookpad.com/ua/recipes/17150293
- Mlynci na molotsi (thin pancakes) — https://cookpad.com/ua/recipes/16389217
- Greek salad — https://cookpad.com/ua/recipes/25020130
- American cookies — https://cookpad.com/ua/recipes/24236789

**Silpo Integration (MVP)**
- Implement Silpo provider in integration layer.
- “Send to Silpo” exports shopping list into Silpo web flow:
- Map each list item to a search query string.
- Open Silpo web catalog/search in system browser or in-app webview.
- Provide one-click copy/share of the whole list for manual search.
- Persist ingredient→product mapping to speed up repeat usage.
- Parallel track: reach out to Silpo for official partner/API access.
- MVP flow detail:
- Build a concatenated query list from the shopping list.
- Open `https://silpo.ua/catalog` and provide copy/share for queries.

**Data & Storage**
- Local SQLite for recipes, meal plan items, shopping list state, mappings.
- Keep storage behind repository interfaces for future sync.

**Milestones**
1. Solution restructure + shared projects, wiring DI.
2. Cookpad recipe data import (curated JSON) + loader service for MAUI Blazor.
3. MAUI Blazor UI: recipe list/detail, meal plan list, shopping list aggregation.
4. Silpo provider + export flow (open catalog + share/copy queries).
5. Blazor Web app: recipe list/detail, meal plan list, shopping list aggregation.
6. QA pass on Android and iOS + web smoke test.

**Risks & Decisions**
- Cookpad usage must respect their terms; MVP uses manual curation.
- Silpo integration is limited without official API; keep it external/web-based.
- Web UI will be similar in functionality, not pixel-identical to mobile MVP.
