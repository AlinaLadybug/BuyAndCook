# Silpo Integration Investigation Plan

1. Prep Playwright capture: confirm target flow (guest vs logged-in), arrange test account/OTP if needed, and set up a Playwright script to record HAR + trace for “search → add to cart”.
2. Run the Playwright scenario and collect artifacts (HAR/trace), then extract candidate requests, required headers, cookies/tokens, and any prerequisite calls.
3. Derive the minimal server-side sequence: required auth/session bootstrap, product lookup, and the add-to-cart request payload and headers.
4. Draft and validate a minimal HttpClient implementation (or sequence), then document the exact endpoints, payloads, and dependencies.
