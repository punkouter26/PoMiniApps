const { chromium } = require('@playwright/test');

(async () => {
    const browser = await chromium.launch();
    const context = await browser.createContext({
        viewport: { width: 1280, height: 720 }
    });
    const page = await context.newPage();

    const pages = [
        // Home and navigation
        { url: 'http://localhost:5000/', name: 'home' },

        // System pages
        { url: 'http://localhost:5000/diag', name: 'diagnostics' },

        // Lingual mini app - Rap Battle
        { url: 'http://localhost:5000/apps/lingual/rap-battle', name: 'rap-battle' },
        { url: 'http://localhost:5000/apps/lingual/leaderboard', name: 'leaderboard' },

        // Lingual mini app - Victorian Translator
        { url: 'http://localhost:5000/apps/lingual/victorian-translator', name: 'victorian-translator' },
    ];

    for (const pageInfo of pages) {
        try {
            console.log(`Capturing ${pageInfo.name}...`);
            await page.goto(pageInfo.url, { waitUntil: 'networkidle', timeout: 15000 });
            await page.waitForTimeout(2000); // Wait for any animations
            await page.screenshot({ path: `screenshots/${pageInfo.name}.png`, fullPage: true });
            console.log(`✓ Captured ${pageInfo.name}`);
        } catch (error) {
            console.error(`✗ Failed to capture ${pageInfo.name}: ${error.message}`);
        }
    }

    await browser.close();
})();
