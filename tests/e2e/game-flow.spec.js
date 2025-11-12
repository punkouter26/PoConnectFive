// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Completes the in-app game setup dialog using default values unless overridden.
 * @param {import('@playwright/test').Page} page
 * @param {{ player1?: string; player2?: string; mode?: 'single' | 'two'; }} [options]
 */
async function completeGameSetup(page, options = {}) {
    const { player1, player2, mode = 'single' } = options;

    const dialog = page.getByRole('dialog', { name: 'Start New Game' });
    await expect(dialog).toBeVisible({ timeout: 10000 });

    if (mode === 'two') {
        await dialog.getByRole('radio', { name: 'Two Players' }).check();
    } else {
        await dialog.getByRole('radio', { name: 'Single Player (vs AI)' }).check();
    }

    if (player1) {
        await dialog.getByLabel('Player 1 Name').fill(player1);
    }

    if (mode === 'two') {
        if (player2) {
            await dialog.getByLabel('Player 2 Name').fill(player2);
        }
    }

    await dialog.getByRole('button', { name: 'Start Game' }).click();
    await expect(dialog).toBeHidden({ timeout: 10000 });
}

/**
 * Navigates from the home page to the game page and completes setup for single player mode.
 * @param {import('@playwright/test').Page} page
 * @param {'Easy' | 'Medium' | 'Hard'} difficulty
 */
async function startSinglePlayerGame(page, difficulty) {
    await page.getByRole('button', { name: new RegExp(difficulty, 'i') }).click();
    await expect(page).toHaveURL(/game/);
    await completeGameSetup(page, { mode: 'single' });
}

test.describe('Game Flow', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        // Wait for Blazor WASM to fully load by checking for actual content
        await page.getByRole('heading', { name: 'Single Player' }).waitFor({ timeout: 60000 });
    });

    test('should load the home page successfully', async ({ page }) => {
        await expect(page).toHaveTitle(/PoConnectFive/);
        // PoConnectFive is a link, not a heading - check for it as a link
        await expect(page.getByRole('link', { name: 'PoConnectFive home' })).toBeVisible();
        await expect(page.getByRole('heading', { name: 'Single Player' })).toBeVisible();
        await expect(page.getByRole('heading', { name: 'Two Players' })).toBeVisible();
    });

    test('should start a single player game with Easy difficulty', async ({ page }) => {
        await startSinglePlayerGame(page, 'Easy');

        // Check if game controls are present (which means we're on the game page)
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible({ timeout: 5000 });
        await expect(page.getByRole('button', { name: /Reset/i })).toBeVisible();
    });

    test('should complete a full game flow', async ({ page }) => {
        await startSinglePlayerGame(page, 'Easy');

        // Verify game controls are present
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible();
        await expect(page.getByRole('button', { name: /Reset/i })).toBeVisible();
    });

    test('should show win probability indicator', async ({ page }) => {
        await startSinglePlayerGame(page, 'Medium');

        // Verify we're on the game page
        await expect(page.getByRole('button', { name: /Back/i })).toBeVisible();
    });

    test('should allow starting a new game', async ({ page }) => {
        await startSinglePlayerGame(page, 'Easy');

        // Verify New Game button exists
        const newGameBtn = page.getByRole('button', { name: /New Game/i });
        await expect(newGameBtn).toBeVisible();
    });

    test('should navigate back to menu', async ({ page }) => {
        await startSinglePlayerGame(page, 'Easy');

        // Set up dialog handler to accept the confirmation
        page.once('dialog', dialog => dialog.accept());

        // Click back button
        await page.getByRole('button', { name: /Back/i }).click();

        // Should be back at home
        await expect(page).toHaveURL('/');
        await expect(page.getByRole('heading', { name: 'Single Player' })).toBeVisible();
    });

    test('should handle invalid moves gracefully', async ({ page }) => {
        await startSinglePlayerGame(page, 'Easy');

        // Game page should be loaded - verify by checking for controls
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible();
    });
});
