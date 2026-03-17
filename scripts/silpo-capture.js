const { chromium } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

const artifactsDir = path.resolve(__dirname, '..', 'artifacts', 'silpo');
fs.mkdirSync(artifactsDir, { recursive: true });

const harPath = path.join(artifactsDir, 'silpo.har');
const tracePath = path.join(artifactsDir, 'silpo-trace.zip');
const screenshotPath = path.join(artifactsDir, 'silpo-after-add.png');

const SEARCH_TERM = process.env.SILPO_SEARCH || 'молоко';

const searchSelectors = [
  'input[type="search"]',
  'input[placeholder*="Пошук"]',
  'input[placeholder*="пошук"]',
  'input[placeholder*="Search"]',
  'input[name*="search"]',
  'input[aria-label*="Пошук"]',
  'input[aria-label*="search"]'
];

const addToCartSelectors = [
  'button:has-text("В кошик")',
  'button:has-text("У кошик")',
  'button:has-text("До кошика")',
  'button:has-text("Купити")',
  'button[aria-label*="кошик"]',
  'button[aria-label*="cart"]',
  'button[data-testid*="cart"]',
  'button[data-testid*="add"]'
];

async function clickIfVisible(page, selectors) {
  for (const selector of selectors) {
    const candidate = page.locator(selector).first();
    try {
      await candidate.waitFor({ state: 'visible', timeout: 2000 });
      await candidate.click({ timeout: 2000 });
      return true;
    } catch (error) {
      // Try next selector.
    }
  }
  return false;
}

async function findFirstVisible(page, selectors) {
  for (const selector of selectors) {
    const candidate = page.locator(selector).first();
    try {
      await candidate.waitFor({ state: 'visible', timeout: 3000 });
      return candidate;
    } catch (error) {
      // Try next selector.
    }
  }
  return null;
}

async function main() {
  const headless = process.env.SILPO_HEADLESS !== 'false';
  const storageStatePath = process.env.SILPO_STORAGE_STATE;
  const browser = await chromium.launch({ headless });
  const contextOptions = {
    recordHar: { path: harPath, content: 'attach' },
    locale: 'uk-UA',
    timezoneId: 'Europe/Kyiv',
    viewport: { width: 1440, height: 900 }
  };

  if (storageStatePath) {
    contextOptions.storageState = storageStatePath;
  }

  const context = await browser.newContext(contextOptions);

  if (process.env.SILPO_COOKIES_JSON) {
    try {
      const cookies = JSON.parse(process.env.SILPO_COOKIES_JSON);
      if (Array.isArray(cookies) && cookies.length > 0) {
        await context.addCookies(cookies);
      }
    } catch (error) {
      console.warn('Failed to parse SILPO_COOKIES_JSON; ignoring.');
    }
  }

  await context.tracing.start({ screenshots: true, snapshots: true, sources: true });

  const page = await context.newPage();
  await page.goto('https://silpo.ua/catalog', { waitUntil: 'domcontentloaded' });

  await clickIfVisible(page, [
    'button:has-text("Прийняти")',
    'button:has-text("Погоджуюсь")',
    'button:has-text("Accept")'
  ]);

  await clickIfVisible(page, [
    'button[aria-label*="Пошук"]',
    'button[aria-label*="пошук"]',
    'button[aria-label*="search"]',
    'button:has-text("Пошук")',
    'button[data-testid*="search"]'
  ]);

  const challengeInput = page.locator('input[name=\"cf-turnstile-response\"]');
  if (await challengeInput.count()) {
    await page.screenshot({ path: path.join(artifactsDir, 'silpo-challenge.png'), fullPage: true });
    throw new Error('Cloudflare challenge detected. Provide SILPO_STORAGE_STATE or SILPO_COOKIES_JSON to continue.');
  }

  const searchInput = await findFirstVisible(page, searchSelectors);
  if (!searchInput) {
    const inputs = await page.$$eval('input', (elements) =>
      elements.map((input) => ({
        type: input.type,
        name: input.name,
        id: input.id,
        placeholder: input.placeholder,
        ariaLabel: input.getAttribute('aria-label'),
        className: input.className
      }))
    );
    fs.writeFileSync(path.join(artifactsDir, 'inputs.json'), JSON.stringify(inputs, null, 2));
    await page.screenshot({ path: path.join(artifactsDir, 'silpo-no-search.png'), fullPage: true });
    throw new Error('Search input not found. Update selectors in scripts/silpo-capture.js.');
  }

  await searchInput.fill(SEARCH_TERM);
  await searchInput.press('Enter');
  await page.waitForTimeout(3000);

  let added = await clickIfVisible(page, addToCartSelectors);
  if (!added) {
    const productCard = page.locator('[class*="product"], [data-testid*="product"]').first();
    if (await productCard.count()) {
      const cardButton = productCard.locator('button').first();
      try {
        await cardButton.waitFor({ state: 'visible', timeout: 3000 });
        await cardButton.click();
        added = true;
      } catch (error) {
        // ignore
      }
    }
  }

  if (!added) {
    throw new Error('Add-to-cart button not found. Update selectors in scripts/silpo-capture.js.');
  }

  await page.waitForTimeout(3000);
  await page.screenshot({ path: screenshotPath, fullPage: true });

  await context.tracing.stop({ path: tracePath });
  await context.close();
  await browser.close();

  console.log(`HAR saved to ${harPath}`);
  console.log(`Trace saved to ${tracePath}`);
  console.log(`Screenshot saved to ${screenshotPath}`);
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
