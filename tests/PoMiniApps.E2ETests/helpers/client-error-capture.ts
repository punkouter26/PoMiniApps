import { type Page } from '@playwright/test';

/**
 * Attaches client error capture listeners to a page and returns collected errors.
 * Captures JavaScript errors and console errors that occur during page interactions.
 * 
 * @param page - The Playwright page object
 * @returns Array of error messages (empty if no errors)
 */
export function attachClientErrorCapture(page: Page): string[] {
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
