const { test, expect } = require('@playwright/test');

test('shows Silpo API tooling', async ({ page }) => {
  await page.goto('/');
  await page.waitForSelector('[data-testid="interactive-ready"]', { state: 'attached' });

  await expect(page.getByRole('heading', { name: 'Silpo API' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Location' })).toBeVisible();
  await expect(page.getByPlaceholder('Enter address (city, street, house)')).toBeVisible();

  const searchButtons = page.getByRole('button', { name: 'Search' });
  await expect(searchButtons).toHaveCount(2);
  await expect(searchButtons.nth(0)).toBeDisabled();
  await expect(searchButtons.nth(1)).toBeDisabled();

  await expect(page.getByRole('heading', { name: 'Cart', exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Create cart' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Refresh' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Clear' })).toBeVisible();

  await expect(page.getByRole('heading', { name: 'Product search' })).toBeVisible();
  await expect(page.getByPlaceholder('Search products in Silpo')).toBeVisible();
});
