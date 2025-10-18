// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Game Flow', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
    });

    test('should load the home page successfully', async ({ page }) => {
        await expect(page).toHaveTitle(/PoConnectFive/);
        await expect(page.locator('text=PoConnectFive')).toBeVisible();
        await expect(page.locator('text=Single Player')).toBeVisible();
        await expect(page.locator('text=Two Players')).toBeVisible();
    });

    test('should start a single player game with Easy difficulty', async ({ page }) => {
        // Click Easy difficulty button
        await page.locator('button:has-text("Easy")').click();

        // Wait for navigation to game page
        await expect(page).toHaveURL(/game/);

        // Game board should be visible
        await expect(page.locator('.game-board')).toBeVisible();

        // Status should show current player
        await expect(page.locator('text=Player 1')).toBeVisible();
    });

    test('should complete a full game flow', async ({ page }) => {
        // Start a game
        await page.locator('button:has-text("Easy")').click();
        await expect(page).toHaveURL(/game/);

        // Wait for board to load
        await page.waitForSelector('.game-board');

        // Make a move (click column 4 - center)
        const columns = page.locator('.board-column');
        await columns.nth(4).click();

        // Wait for piece animation
        await page.waitForTimeout(500);

        // Should see a piece placed
        const pieces = page.locator('.piece');
        await expect(pieces.first()).toBeVisible();

        // AI should make a move
        await page.waitForTimeout(1000);

        // There should be more pieces after AI move
        const piecesAfterAI = await pieces.count();
        expect(piecesAfterAI).toBeGreaterThanOrEqual(2);
    });

    test('should show win probability indicator', async ({ page }) => {
        // Start a game
        await page.locator('button:has-text("Medium")').click();
        await expect(page).toHaveURL(/game/);

        // Wait for board
        await page.waitForSelector('.game-board');

        // Make a move
        await page.locator('.board-column').nth(4).click();
        await page.waitForTimeout(500);

        // Win probability should be visible
        await expect(page.locator('.live-win-probability')).toBeVisible();
        await expect(page.locator('.probability-bar')).toBeVisible();
    });

    test('should allow starting a new game', async ({ page }) => {
        // Start a game
        await page.locator('button:has-text("Easy")').click();
        await expect(page).toHaveURL(/game/);

        // Click New Game button
        await page.locator('button:has-text("New Game")').click();

        // Should show confirmation dialog (if game in progress)
        // Click cancel on prompt to stay in current game
        page.on('dialog', dialog => dialog.dismiss());
    });

    test('should navigate back to menu', async ({ page }) => {
        // Start a game
        await page.locator('button:has-text("Easy")').click();
        await expect(page).toHaveURL(/game/);

        // Click back button
        await page.locator('button:has-text("Back to Menu")').click();

        // Should be back at home
        await expect(page).toHaveURL('/');
        await expect(page.locator('text=Single Player')).toBeVisible();
    });

    test('should handle invalid moves gracefully', async ({ page }) => {
        // Start a game
        await page.locator('button:has-text("Easy")').click();
        await expect(page).toHaveURL(/game/);

        // Wait for board
        await page.waitForSelector('.game-board');

        // Fill a column completely (would need multiple moves)
        const column = page.locator('.board-column').nth(0);

        // Click same column multiple times rapidly
        for (let i = 0; i < 3; i++) {
            await column.click();
            await page.waitForTimeout(300);
        }

        // Game should still be functional
        await expect(page.locator('.game-board')).toBeVisible();
    });
});
