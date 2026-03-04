import { expect, test, type Page } from '@playwright/test';

function attachClientErrorCapture(page: Page): string[] {
  const clientErrors: string[] = [];

  page.on('pageerror', (error) => {
    clientErrors.push(`pageerror: ${error.message}`);
  });

  page.on('console', (message) => {
    if (message.type() === 'error') {
      clientErrors.push(`console.error: ${message.text()}`);
    }
  });

  return clientErrors;
}

test.describe('Home Page', () => {
  test('should display the PoMiniApps title', async ({ page }) => {
    const clientErrors = attachClientErrorCapture(page);

    await page.goto('/');

    await expect(page.getByRole('heading', { name: /PoMiniApps/i })).toBeVisible();
    expect(clientErrors).toEqual([]);
  });

  test('should show mini app cards and utilities', async ({ page }) => {
    const clientErrors = attachClientErrorCapture(page);

    await page.goto('/');

    await expect(page.getByText('Rap Battle Arena')).toBeVisible();
    await expect(page.getByText('Victorian Translator')).toBeVisible();
    // Check that at least one Launch button is visible (there are now 2 apps)
    await expect(page.getByRole('button', { name: /Launch/i }).first()).toBeVisible();
    await expect(page.getByRole('button', { name: /Diagnostics/i })).toBeVisible();
    expect(clientErrors).toEqual([]);
  });

  test('should load lingual app route', async ({ page }) => {
    const clientErrors = attachClientErrorCapture(page);

    await page.goto('/apps/lingual');

    await expect(page).toHaveURL(/.*apps\/lingual/);
    await expect(page.getByRole('heading', { name: /Lingual Playground/i })).toBeVisible();
    expect(clientErrors).toEqual([]);
  });

  test('should navigate to diagnostics page directly', async ({ page }) => {
    const clientErrors = attachClientErrorCapture(page);

    await page.goto('/diag');

    await expect(page.getByRole('heading', { name: /Diagnostics/i })).toBeVisible();
    expect(clientErrors).toEqual([]);
  });
});
