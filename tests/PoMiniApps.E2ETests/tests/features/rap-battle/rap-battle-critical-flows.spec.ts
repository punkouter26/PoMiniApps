import { expect, test } from '@playwright/test';
import { attachClientErrorCapture } from '../../../helpers/client-error-capture';

test.describe('Rap Battle Critical Flows', () => {
    test('should load rap battle setup flow', async ({ page }) => {
        const clientErrors = attachClientErrorCapture(page);

        await page.goto('/apps/lingual/rap-battle');

        await expect(page.getByRole('heading', { name: /Rap Battle Arena/i })).toBeVisible();
        await expect(page.getByText('Set Up Your Battle')).toBeVisible();
        await expect(page.getByRole('button', { name: /Start Battle/i })).toBeVisible();
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
});
