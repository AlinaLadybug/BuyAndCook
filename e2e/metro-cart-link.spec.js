const { test, expect } = require('@playwright/test');

test('creates a live Metro cart and prepares a Metro restore handoff', async ({ page }) => {
  test.setTimeout(180000);

  await page.goto('/');
  await page.waitForSelector('[data-testid="interactive-ready"]', { state: 'attached' });

  const addButtons = page.getByRole('button', { name: 'Add to plan' });
  const recipeCount = Math.min(await addButtons.count(), 2);
  for (let index = 0; index < recipeCount; index += 1) {
    await addButtons.nth(index).click();
  }

  await expect(page.getByRole('button', { name: 'Remove' }).first()).toBeVisible();

  await page.getByRole('button', { name: 'Generate' }).click();
  await expect(page.locator('.shopping li').first()).toBeVisible();

  const metroCard = page.getByRole('heading', { name: 'Metro cart link' }).locator('..');
  await expect(metroCard).toBeVisible();

  const buildButton = page.getByRole('button', { name: 'Create Metro cart link' });
  await expect(buildButton).toBeEnabled();
  await buildButton.click();

  await expect(metroCard.getByText(/Added .* to Metro cart\./)).toBeVisible({ timeout: 120000 });
  await expect(metroCard.getByText('Unable to build Metro cart.')).toHaveCount(0);

  const cartLink = metroCard.locator('a.link').filter({ hasText: '/metro/cart/' });
  await expect(cartLink).toBeVisible();
  const shareUrl = await cartLink.getAttribute('href');
  expect(shareUrl).toContain('/metro/cart/');

  const restoreButton = metroCard.getByTestId('metro-restore-bookmarklet-copy');
  await expect(restoreButton).toBeVisible();

  const bookmarklet = await restoreButton.getAttribute('data-bookmarklet');
  expect(bookmarklet).toBeTruthy();
  expect(bookmarklet).toContain("javascript:(()=>{");
  expect(bookmarklet).toContain("localStorage.setItem('anonymousCartId'");
  expect(bookmarklet).toContain("localStorage.setItem('anonymousCartStore'");
  expect(bookmarklet).toContain("document.cookie='anonymousUserId='");
  expect(bookmarklet).toContain("document.cookie='SES2_customerAdr_='");
  expect(bookmarklet).toContain("MShopCookieCategoriesC");
  expect(bookmarklet).toContain("MShopCookieCategoriesL");

  await page.goto(shareUrl);
  await expect(page.getByRole('heading', { name: 'Metro Cart' })).toBeVisible();
  await expect(page.locator('.silpo-cart-item').first()).toBeVisible({ timeout: 60000 });

  const firstItemText = (await page.locator('.silpo-cart-item span').first().textContent())?.trim();
  expect(firstItemText).toBeTruthy();
});
