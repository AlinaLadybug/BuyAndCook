# Silpo add-to-cart investigation

## Status (2026-02-21)
- Attempted headless Playwright capture at https://silpo.ua/catalog.
- Blocked by Cloudflare Turnstile challenge (only `cf-turnstile-response` input present).
- No HAR/trace captured yet, so endpoints/payloads are not available.

## What is prepared
- `scripts/silpo-capture.js`: records HAR + trace + screenshot for the flow “search → add to cart”.
- `scripts/silpo-analyze-har.js`: creates a sanitized summary of relevant requests.
- `artifacts/` is gitignored for sensitive captures.

## How to unblock capture
Option A (recommended): create a Playwright storage state after passing the challenge in a visible browser.

```bash
npx playwright codegen --save-storage=artifacts/silpo/storage-state.json https://silpo.ua/catalog
```

Then run:

```bash
SILPO_STORAGE_STATE=/absolute/path/to/artifacts/silpo/storage-state.json node scripts/silpo-capture.js
node scripts/silpo-analyze-har.js
```

Option B: provide cookies as JSON (same format as Playwright cookies).

```bash
SILPO_COOKIES_JSON='[{"name":"...","value":"...","domain":".silpo.ua","path":"/","httpOnly":true,"secure":true}]' node scripts/silpo-capture.js
node scripts/silpo-analyze-har.js
```

## Next steps once capture is available
- Parse HAR for cart-related endpoints, payloads, headers, and required cookies/tokens.
- Draft minimal `HttpClient` request sequence that reproduces “add to cart”.
- Document exact dependencies and pre-requests.
