const { test, expect, chromium } = require('@playwright/test');

const useStub = process.env.SILPO_API_STUB === 'true';

test('creates a live Silpo cart and prepares a real silpo.ua basket handoff', async ({ page }, testInfo) => {
  test.skip(useStub, 'Requires real Silpo API.');
  test.setTimeout(180000);

  await page.goto('/');
  await page.waitForSelector('[data-testid="interactive-ready"]', { state: 'attached' });

  const addButtons = page.getByRole('button', { name: 'Add to plan' });
  if (await addButtons.count() > 0) {
    await addButtons.first().click();
  }

  await expect(page.getByRole('button', { name: 'Remove' }).first()).toBeVisible();

  await page.getByRole('button', { name: 'Generate' }).click();
  await expect(page.locator('.shopping li').first()).toBeVisible();

  const cartLinkHeading = page.getByRole('heading', { name: 'Silpo cart link' });
  await expect(cartLinkHeading).toBeVisible();

  const createCartButton = page.getByRole('button', { name: 'Create cart' });
  const hasSavedLocation = await createCartButton.isEnabled();

  if (!hasSavedLocation) {
    const addressInput = page.getByPlaceholder('Enter address (city, street, house)');
    const testAddress = process.env.SILPO_E2E_ADDRESS || 'Kyiv, Khreshchatyk 10';

    expect(testAddress).toMatch(/\d/);
    await addressInput.fill(testAddress);
    await addressInput.blur();

    const locationSearchButton = addressInput.locator('..').getByRole('button', { name: 'Search' });
    await expect(locationSearchButton).toBeEnabled();
    await locationSearchButton.click();

    const suggestions = page.locator('.silpo-suggestion');
    await expect(suggestions.first()).toBeVisible({ timeout: 20000 });

    const suggestionsWithHouse = suggestions.filter({ hasText: /\d/ });
    const targetSuggestion = (await suggestionsWithHouse.count()) > 0
      ? suggestionsWithHouse.first()
      : suggestions.first();

    await targetSuggestion.getByRole('button', { name: 'Use' }).click();
    await expect(page.getByText('Nearest branch:')).toBeVisible({ timeout: 20000 });
  } else {
    await expect(page.getByText('Loaded saved location.')).toBeVisible();
  }

  const buildButton = page.getByRole('button', { name: 'Create Silpo cart link' });
  await expect(buildButton).toBeEnabled();
  await buildButton.click();

  const cartCard = cartLinkHeading.locator('..');
  await expect(cartCard.getByText(/Added|Unable to build Silpo cart\./)).toBeVisible({ timeout: 60000 });
  await expect(cartCard.getByText('Unable to build Silpo cart.')).toHaveCount(0);

  const cartLink = cartCard.locator('a.link').filter({ hasText: '/silpo/cart/' });
  await expect(cartLink).toBeVisible();
  await expect(cartLink).toHaveAttribute('href', /\/silpo\/cart\//);

  const restoreButton = cartCard.getByTestId('silpo-restore-bookmarklet-copy').first();
  await expect(restoreButton).toBeVisible();

  const bookmarklet = await restoreButton.getAttribute('data-bookmarklet');
  expect(bookmarklet).toBeTruthy();
  expect(bookmarklet).toContain("javascript:(()=>{");

  const cartIdMatch = bookmarklet.match(/localStorage\.setItem\('basketId','([^']+)'\)/);
  const branchIdMatch = bookmarklet.match(/document\.cookie='branchId=([^;']+)/);
  expect(cartIdMatch).toBeTruthy();
  expect(branchIdMatch).toBeTruthy();

  const cartId = cartIdMatch[1];
  const branchId = branchIdMatch[1];
  const cartItemTitle = (await page.locator('.silpo-cart-item span').first().textContent())?.trim();
  expect(cartItemTitle).toBeTruthy();

  const silpoBrowser = await chromium.launch({ headless: false });
  try {
    const silpoContext = await silpoBrowser.newContext({ viewport: { width: 1440, height: 1080 } });
    await silpoContext.addCookies([
      {
        name: 'branchId',
        value: branchId,
        domain: 'silpo.ua',
        path: '/'
      },
      {
        name: 'disable-ssr',
        value: 'yes',
        domain: 'silpo.ua',
        path: '/'
      }
    ]);
    await silpoContext.addInitScript((basketId) => {
      window.localStorage.setItem('basketId', basketId);
    }, cartId);

    const silpoPage = await silpoContext.newPage();
    await silpoPage.goto('https://silpo.ua/basket', { waitUntil: 'domcontentloaded', timeout: 120000 });
    await silpoPage.waitForTimeout(12000);

    await expect(silpoPage.getByText(cartItemTitle, { exact: false })).toBeVisible({ timeout: 60000 });
    await silpoPage.screenshot({
      path: testInfo.outputPath('silpo-basket-restored.png'),
      fullPage: true
    });

    const liveBasketId = await silpoPage.evaluate(() => window.localStorage.getItem('basketId'));
    expect(liveBasketId).toBe(cartId);
  } finally {
    await silpoBrowser.close();
  }
});
