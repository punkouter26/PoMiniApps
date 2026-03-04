import { expect, test } from '@playwright/test';

test.describe('Translator Full Submission Flow', () => {
    test('should navigate to translator page successfully', async ({ page }) => {
        await page.goto('/apps/lingual/victorian-translator');
        await page.waitForLoadState('networkidle');

        // Verify page loaded with heading
        await expect(page.getByRole('heading', { name: /Victorian English Translator/i })).toBeVisible();
    });

    test('should render translator with buttons available', async ({ page }) => {
        await page.goto('/apps/lingual/victorian-translator');
        await page.waitForLoadState('networkidle');

        // Verify buttons exist (translate action button)
        const buttons = page.locator('button');
        const count = await buttons.count();
        expect(count).toBeGreaterThan(0);
    });

    test('should have input form for text translation', async ({ page }) => {
        await page.goto('/apps/lingual/victorian-translator');
        await page.waitForLoadState('networkidle');

        // Verify textarea exists for text input
        const textarea = page.locator('textarea');
        expect(await textarea.count()).toBeGreaterThan(0);
    });

    test('should display translator page without JavaScript errors', async ({ page }) => {
        const errors: string[] = [];

        page.on('pageerror', (error) => {
            errors.push(error.message);
        });

        await page.goto('/apps/lingual/victorian-translator');
        await page.waitForLoadState('networkidle');

        // Verify heading loads
        await expect(page.getByRole('heading', { name: /Victorian English Translator/i })).toBeVisible();

        // No unhandled JavaScript errors
        expect(errors).toEqual([]);
    });
});

