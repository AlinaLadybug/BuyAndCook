const { test, expect } = require('@playwright/test');

test('creates a Silpo cart link from the shopping list', async ({ page }) => {
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

  const addressInput = page.getByPlaceholder('Enter address (city, street, house)');
  await addressInput.fill('Kyiv, Test Street 10');
  await addressInput.blur();

  const locationSearchButton = addressInput.locator('..').getByRole('button', { name: 'Search' });
  await expect(locationSearchButton).toBeEnabled();
  await locationSearchButton.click();

  const useButton = page.getByRole('button', { name: 'Use' }).first();
  await useButton.click();

  await expect(page.getByText('Nearest branch:')).toBeVisible();

  const buildButton = page.getByRole('button', { name: 'Create Silpo cart link' });
  await expect(buildButton).toBeEnabled();
  await buildButton.click();

  const cartCard = cartLinkHeading.locator('..');
  await expect(cartCard.getByText('Added')).toBeVisible();

  const cartLink = cartCard.locator('a.link').filter({ hasText: '/silpo/cart/' });
  await expect(cartLink).toBeVisible();
  await expect(cartLink).toHaveAttribute('href', /\/silpo\/cart\//);
});
