const { test, expect } = require('@playwright/test');

test('builds a meal plan and generates a shopping list', async ({ page }) => {
  await page.goto('/');
  await page.waitForSelector('[data-testid=\"interactive-ready\"]', { state: 'attached' });

  await expect(page.getByRole('heading', { name: 'Recipes' })).toBeVisible();
  const cards = page.locator('.card');
  await expect(cards.first()).toBeVisible();

  const addButtons = page.getByRole('button', { name: 'Add to plan' });
  await addButtons.first().click();
  await expect(page.getByRole('button', { name: 'Remove' }).first()).toBeVisible();
  await expect(page.locator('.plan li').first()).toBeVisible();

  const generateButton = page.getByRole('button', { name: 'Generate' });
  await expect(generateButton).toBeEnabled();
  await generateButton.click();

  const shoppingItems = page.locator('.shopping li');
  await expect(shoppingItems.first()).toBeVisible();

  const queries = page.locator('.queries');
  await expect(queries).toBeVisible();

  const mappingHeading = page.getByRole('heading', { name: 'Silpo search terms' });
  await expect(mappingHeading).toBeVisible();

  const mappingInput = page.locator('.mapping-item input').first();
  await mappingInput.fill('Test paprika');
  await expect(page.getByRole('button', { name: 'Save terms' })).toBeEnabled();
  await expect(queries).toContainText('Test paprika');

  const saveButton = page.getByRole('button', { name: 'Save terms' });
  await saveButton.click();
  await expect(saveButton).toBeDisabled();
});
