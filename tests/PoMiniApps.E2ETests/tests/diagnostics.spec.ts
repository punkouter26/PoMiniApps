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

test.describe('Diagnostics Page', () => {
  test('should display diagnostics heading', async ({ page }) => {
    const clientErrors = attachClientErrorCapture(page);

    await page.goto('/diag');
    await expect(page.getByRole('heading', { name: /Diagnostics/i })).toBeVisible();
    expect(clientErrors).toEqual([]);
  });

  test('should have run checks button', async ({ page }) => {
    const clientErrors = attachClientErrorCapture(page);

    await page.goto('/diag');
    await expect(page.getByRole('button', { name: /Run All Checks/i })).toBeVisible();
    expect(clientErrors).toEqual([]);
  });
});
