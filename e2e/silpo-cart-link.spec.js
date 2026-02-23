const { test, expect } = require('@playwright/test');

const useStub = process.env.SILPO_API_STUB === 'true';

test('creates a Silpo cart link from the shopping list', async ({ page }) => {
  test.skip(useStub, 'Requires real Silpo API.');
  await page.goto('/');
  await page.waitForSelector('[data-testid="interactive-ready"]', { state: 'attached' });

  const addButtons = page.getByRole('button', { name: 'Add to plan' });
  if (await addButtons.count() > 0) {
    await addButtons.first().click();
  }
  await expect(page.getByRole('button', { name: 'Remove' }).first()).toBeVisible();

  const generateButton = page.getByRole('button', { name: 'Generate' });
  await generateButton.click();
  await expect(page.locator('.shopping li').first()).toBeVisible();

  const cartLinkHeading = page.getByRole('heading', { name: 'Silpo cart link' });
  await expect(cartLinkHeading).toBeVisible();

  test.setTimeout(120000);

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
    const suggestionCount = await suggestionsWithHouse.count();
    const targetSuggestion = suggestionCount > 0 ? suggestionsWithHouse.first() : suggestions.first();
    const useButton = targetSuggestion.getByRole('button', { name: 'Use' });
    await useButton.click();

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
});
