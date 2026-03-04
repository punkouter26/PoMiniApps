import { expect, test } from '@playwright/test';
import { attachClientErrorCapture } from '../../../helpers/client-error-capture';

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
