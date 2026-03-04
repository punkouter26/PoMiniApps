import { expect, test } from '@playwright/test';
import { attachClientErrorCapture } from '../helpers/client-error-capture';

test.describe('Mobile and Accessibility', () => {
    test.use({ viewport: { width: 390, height: 844 } });

    test('should render Lingual hub on mobile viewport', async ({ page }) => {
        const clientErrors = attachClientErrorCapture(page);

        await page.goto('/apps/lingual');

        await expect(page.getByRole('heading', { name: /Lingual Playground/i })).toBeVisible();
        await expect(page.getByRole('button', { name: /Enter Arena/i })).toBeVisible();
        await expect(page.getByRole('button', { name: /Start Translating/i })).toBeVisible();
        expect(clientErrors).toEqual([]);
    });

    test('should expose core controls with accessible names', async ({ page }) => {
        const clientErrors = attachClientErrorCapture(page);

        await page.goto('/apps/lingual/victorian-translator');

        await expect(page.getByRole('button', { name: /Random Lyrics/i })).toBeVisible();
        await expect(page.getByRole('button', { name: /Translate to Victorian/i })).toBeVisible();
        await expect(page.getByPlaceholder('Enter modern English text to translate...')).toBeVisible();
        expect(clientErrors).toEqual([]);
    });
});
