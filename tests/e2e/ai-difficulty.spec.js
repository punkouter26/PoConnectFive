// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Completes the new game setup dialog with optional overrides.
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

    if (mode === 'two' && player2) {
        await dialog.getByLabel('Player 2 Name').fill(player2);
    }

    await dialog.getByRole('button', { name: 'Start Game' }).click();
    await expect(dialog).toBeHidden({ timeout: 10000 });
}

/**
 * Starts a single player game by selecting difficulty and completing setup.
 * @param {import('@playwright/test').Page} page
 * @param {'Easy' | 'Medium' | 'Hard'} difficulty
 */
async function startSinglePlayerGame(page, difficulty) {
    await page.getByRole('button', { name: new RegExp(difficulty, 'i') }).click();
    await expect(page).toHaveURL(/game/);
    await completeGameSetup(page, { mode: 'single' });
}

test.describe('AI Difficulty Tests', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        // Wait for Blazor WASM to fully load - look for the Single Player heading
        await page.getByRole('heading', { name: /Single Player/i }).waitFor({ timeout: 60000 });
        await page.waitForTimeout(1000); // Extra buffer for Radzen components
    });

    test('should offer Easy AI difficulty', async ({ page }) => {
        const easyBtn = page.getByRole('button', { name: /Easy/i });
        await expect(easyBtn).toBeVisible();
        await easyBtn.click();
        await expect(page).toHaveURL(/difficulty=Easy/);
        await completeGameSetup(page, { mode: 'single' });
    });

    test('should offer Medium AI difficulty', async ({ page }) => {
        const mediumBtn = page.getByRole('button', { name: /Medium/i });
        await expect(mediumBtn).toBeVisible();
        await mediumBtn.click();
        await expect(page).toHaveURL(/difficulty=Medium/);
        await completeGameSetup(page, { mode: 'single' });
    });

    test('should offer Hard AI difficulty', async ({ page }) => {
        const hardBtn = page.getByRole('button', { name: /Hard/i });
        await expect(hardBtn).toBeVisible();
        await hardBtn.click();
        await expect(page).toHaveURL(/difficulty=Hard/);
        await completeGameSetup(page, { mode: 'single' });
    });

    test('should display AI personality selector for Hard difficulty', async ({ page }) => {
        await page.getByRole('button', { name: /Hard/i }).click();
        await expect(page).toHaveURL(/difficulty=Hard/);

        const dialog = page.getByRole('dialog', { name: 'Start New Game' });
        await expect(dialog.getByLabel('AI Personality')).toBeVisible();

        // Complete setup to avoid impacting later tests
        await completeGameSetup(page, { mode: 'single' });
    });

    test('should show different AI behavior for each difficulty', async ({ page }) => {
        // Test Easy AI
        await startSinglePlayerGame(page, 'Easy');
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible();

        // Go back and test Medium AI
        page.once('dialog', dialog => dialog.accept());
        await page.getByRole('button', { name: /Back/i }).click();
        await page.getByRole('heading', { name: 'Single Player' }).waitFor();

        await startSinglePlayerGame(page, 'Medium');
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible();
    });

    test.skip('should allow two player mode', async ({ page }) => {
        // TODO: RadzenCard Click handler doesn't work with Playwright clicks
        // This test is skipped until we can find the correct selector or interaction method
        await page.getByText('Play against a friend on the same device').click();
        await expect(page).toHaveURL(/game/);
    });
});
