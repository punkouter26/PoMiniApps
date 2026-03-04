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

test.describe('Critical User Flows', () => {
    test('should load rap battle setup flow', async ({ page }) => {
        const clientErrors = attachClientErrorCapture(page);

        await page.goto('/apps/lingual/rap-battle');

        await expect(page.getByRole('heading', { name: /Rap Battle Arena/i })).toBeVisible();
        await expect(page.getByText('Set Up Your Battle')).toBeVisible();
        await expect(page.getByRole('button', { name: /Start Battle/i })).toBeVisible();
        expect(clientErrors).toEqual([]);
    });

    test('should load translator flow and enable translation action', async ({ page }) => {
        const clientErrors = attachClientErrorCapture(page);

        await page.goto('/apps/lingual/victorian-translator');

        await expect(page.getByRole('heading', { name: /Victorian English Translator/i })).toBeVisible();
        const input = page.locator('textarea').first();
        await input.fill('hello world');
        await input.press('Tab');
        await expect(page.getByRole('button', { name: /Translate to Victorian/i })).toBeVisible();
        expect(clientErrors).toEqual([]);
    });

    test('should load leaderboard and allow refresh action', async ({ page }) => {
        const clientErrors = attachClientErrorCapture(page);

        await page.goto('/apps/lingual/leaderboard');

        await expect(page.getByRole('heading', { name: /Rap Battle Leaderboard/i })).toBeVisible();
        await page.getByRole('button', { name: /Refresh/i }).click();
        await expect(page.getByRole('button', { name: /Refresh/i })).toBeVisible();
        expect(clientErrors).toEqual([]);
    });

    test('should run diagnostics checks from diagnostics page', async ({ page }) => {
        const clientErrors = attachClientErrorCapture(page);

        await page.goto('/diag');

        await page.getByRole('button', { name: /Run All Checks/i }).click();
        await expect(page.getByRole('heading', { name: /Diagnostics/i })).toBeVisible();
        expect(clientErrors).toEqual([]);
    });
});
