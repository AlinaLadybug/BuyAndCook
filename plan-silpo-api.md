# Plan - Silpo API Integration (Search, Cart, Location)

## Goals
- Add Silpo API integration for product search, cart creation, add-to-cart, and cart retrieval.
- Resolve user location to nearest delivery branch and store that selection.
- Provide a usable UI flow in both Mobile and Web to search, add items, and view cart state.

## Non-goals
- Checkout and payment (requires OAuth).
- Loyalty/profile integrations.

## Inputs
- Silpo API reference from `/Users/ohaievskyi/Downloads/Telegram Desktop/SILPO_API.md`.

## Steps
1. Define Application-layer contracts and models.
   - Add Silpo settings/options, location, product, and cart models.
   - Define interfaces for location lookup, product search, and cart operations.
   - Add a small session repository to persist branch/location/cart ID.

2. Implement Infrastructure HTTP clients and Silpo API services.
   - Configure HttpClient defaults with required headers.
   - Implement address search, branch lookup, product search, cart create/add/get/update.
   - Map DTOs to Application models and normalize fields.

3. Wire dependency injection and configuration.
   - Register settings, clients, services, and repositories in Web and Mobile.
   - Add configuration defaults and allow overrides for Web via appsettings.

4. Update UI flows (Web + Mobile).
   - Add location selector (address search + branch display).
   - Add product search with add-to-cart actions.
   - Add cart viewer showing items and totals.
   - Keep existing “Send to Silpo” export as fallback.

5. Verification and docs.
   - Run build/tests.
   - Add usage notes and manual QA checklist.
