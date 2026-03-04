import { expect, test } from '@playwright/test';
import { attachClientErrorCapture } from '../helpers/client-error-capture';

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
