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

  const buildButton = page.getByRole('button', { name: 'Create Silpo cart link' });
  await expect(buildButton).toBeEnabled();
  await buildButton.click();

  await expect(page.getByText('Select a location before building a cart.')).toBeVisible();
});
