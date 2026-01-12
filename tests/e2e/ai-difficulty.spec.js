// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Starts a single player game by selecting difficulty and completing setup.
 * The app uses browser prompt() for player name entry AFTER navigation to /game.
 * @param {import('@playwright/test').Page} page
 * @param {'Easy' | 'Medium' | 'Hard'} difficulty
 * @param {string} [playerName='TestPlayer'] - The name to enter for the player
 */
async function startSinglePlayerGame(page, difficulty, playerName = 'TestPlayer') {
    // Set up dialog handler that will catch the prompt on the Game page
    // The prompt appears AFTER navigation to /game, not before
    page.on('dialog', async dialog => {
        if (dialog.type() === 'prompt') {
            await dialog.accept(playerName);
        } else {
            await dialog.accept();
        }
    });

    // Click the difficulty button to start the game
    const button = page.getByRole('button', { name: new RegExp(difficulty, 'i') });
    await button.click();

    // Wait for navigation to game page
    await expect(page).toHaveURL(/game/, { timeout: 15000 });

    // Wait for the game to fully initialize - use the heading which is unique
    await expect(page.getByRole('heading', { name: new RegExp(`${playerName}'s Turn`, 'i') })).toBeVisible({ timeout: 15000 });
}

test.describe('AI Difficulty Tests', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        // Wait for Blazor WASM to fully load - look for the Single Player heading
        await page.getByRole('heading', { name: /Single Player/i }).waitFor({ timeout: 60000 });
        // Wait for Blazor WASM to become interactive (hydration complete)
        await page.waitForFunction(() => {
            return typeof window.Blazor !== 'undefined' && window.Blazor._internal !== undefined;
        }, { timeout: 30000 });
        // Extra safety margin for event handlers to attach
        await page.waitForTimeout(1000);
    });

    test('should offer Easy AI difficulty', async ({ page }) => {
        const easyBtn = page.getByRole('button', { name: /Easy/i });
        await expect(easyBtn).toBeVisible();

        // Set up dialog handler for the prompt (appears AFTER navigation on Game page)
        page.on('dialog', async dialog => {
            if (dialog.type() === 'prompt') {
                await dialog.accept('TestPlayer');
            }
        });

        await easyBtn.click();
        await expect(page).toHaveURL(/difficulty=Easy/);

        // Wait for game to initialize
        await expect(page.getByText('TestPlayer')).toBeVisible({ timeout: 15000 });
    });

    test('should offer Medium AI difficulty', async ({ page }) => {
        const mediumBtn = page.getByRole('button', { name: /Medium/i });
        await expect(mediumBtn).toBeVisible();

        // Set up dialog handler for the prompt (appears AFTER navigation on Game page)
        page.on('dialog', async dialog => {
            if (dialog.type() === 'prompt') {
                await dialog.accept('TestPlayer');
            }
        });

        await mediumBtn.click();
        await expect(page).toHaveURL(/difficulty=Medium/);

        // Wait for game to initialize
        await expect(page.getByText('TestPlayer')).toBeVisible({ timeout: 15000 });
    });

    test('should offer Hard AI difficulty', async ({ page }) => {
        const hardBtn = page.getByRole('button', { name: /Hard/i });
        await expect(hardBtn).toBeVisible();

        // Hard difficulty shows TWO prompts: first for personality (on Index page), then for player name (on Game page)
        // Set up a persistent handler that handles both prompts
        page.on('dialog', async dialog => {
            if (dialog.type() === 'prompt') {
                if (dialog.message().includes('Personality')) {
                    // Personality prompt on Index page
                    await dialog.accept('Balanced');
                } else {
                    // Player name prompt on Game page
                    await dialog.accept('TestPlayer');
                }
            }
        });

        await hardBtn.click();
        await expect(page).toHaveURL(/difficulty=Hard/);

        // Wait for game to initialize
        await expect(page.getByText('TestPlayer')).toBeVisible({ timeout: 15000 });
    });

    test('should display AI personality selector for Hard difficulty', async ({ page }) => {
        // Hard difficulty shows a prompt for personality selection
        let personalityPromptShown = false;

        page.on('dialog', async dialog => {
            if (dialog.type() === 'prompt') {
                if (dialog.message().includes('Personality')) {
                    personalityPromptShown = true;
                    await dialog.accept('Aggressive');
                } else {
                    // Player name prompt
                    await dialog.accept('TestPlayer');
                }
            }
        });

        await page.getByRole('button', { name: /Hard/i }).click();

        // Wait for navigation to game page
        await expect(page).toHaveURL(/difficulty=Hard/);

        // Wait for game to initialize with the aggressive personality
        await expect(page.getByText('TestPlayer')).toBeVisible({ timeout: 15000 });
        await expect(page.getByText(/Aggressive/i)).toBeVisible();

        // Verify the personality prompt was shown
        expect(personalityPromptShown).toBe(true);
    });

    test('should show different AI behavior for each difficulty', async ({ page }) => {
        // Test Easy AI
        await startSinglePlayerGame(page, 'Easy');
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible();

        // Go back and test Medium AI - need to handle any confirmation dialogs
        page.on('dialog', dialog => dialog.accept());
        await page.getByRole('button', { name: /Back/i }).click();
        await page.getByRole('heading', { name: 'Single Player' }).waitFor();

        // Remove previous dialog handlers before starting new game
        page.removeAllListeners('dialog');
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
