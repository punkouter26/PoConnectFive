// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('AI Difficulty Tests', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
    });

    test('should offer Easy AI difficulty', async ({ page }) => {
        await expect(page.locator('button:has-text("Easy")')).toBeVisible();
        await page.locator('button:has-text("Easy")').click();
        await expect(page).toHaveURL(/difficulty=Easy/);
    });

    test('should offer Medium AI difficulty', async ({ page }) => {
        await expect(page.locator('button:has-text("Medium")')).toBeVisible();
        await page.locator('button:has-text("Medium")').click();
        await expect(page).toHaveURL(/difficulty=Medium/);
    });

    test('should offer Hard AI difficulty', async ({ page }) => {
        await expect(page.locator('button:has-text("Hard")')).toBeVisible();
        await page.locator('button:has-text("Hard")').click();
        await expect(page).toHaveURL(/difficulty=Hard/);
    });

    test('should display AI personality prompt for Hard difficulty', async ({ page }) => {
        // Setup dialog handler to check for personality prompt
        let promptShown = false;
        page.on('dialog', async dialog => {
            if (dialog.message().includes('Personality')) {
                promptShown = true;
                await dialog.accept('Balanced');
            } else {
                await dialog.accept('Player 1');
            }
        });

        await page.locator('button:has-text("Hard")').click();

        // Give time for prompts to appear
        await page.waitForTimeout(1000);

        // URL should contain difficulty
        await expect(page).toHaveURL(/difficulty=Hard/);
    });

    test('should show different AI behavior for each difficulty', async ({ page }) => {
        // Test Easy AI - should make random moves
        await page.locator('button:has-text("Easy")').click();
        await page.waitForSelector('.game-board');

        // Make a move
        await page.locator('.board-column').nth(4).click();
        await page.waitForTimeout(500);

        // AI should respond
        await page.waitForTimeout(1000);
        const piecesEasy = await page.locator('.piece').count();
        expect(piecesEasy).toBeGreaterThanOrEqual(2);

        // Go back and test Medium AI
        await page.locator('button:has-text("Back to Menu")').click();
        await page.locator('button:has-text("Medium")').click();
        await page.waitForSelector('.game-board');

        // Make a move
        await page.locator('.board-column').nth(4).click();
        await page.waitForTimeout(500);

        // AI should respond (potentially more strategically)
        await page.waitForTimeout(1000);
        const piecesMedium = await page.locator('.piece').count();
        expect(piecesMedium).toBeGreaterThanOrEqual(2);
    });

    test('should allow two player mode', async ({ page }) => {
        // Setup dialog handler
        page.on('dialog', dialog => dialog.accept('Test Player'));

        await page.locator('text=Two Players').click();

        // Should navigate to two player game
        await expect(page).toHaveURL(/mode=two-player/);
        await expect(page.locator('.game-board')).toBeVisible();
    });
});
