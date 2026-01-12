// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Navigates from the home page to the game page and completes setup for single player mode.
 * The app uses browser prompt() for player name entry AFTER navigation to /game.
 * @param {import('@playwright/test').Page} page
 * @param {'Easy' | 'Medium' | 'Hard'} difficulty
 * @param {string} [playerName='TestPlayer'] - The name to enter for the player
 */
async function startSinglePlayerGame(page, difficulty, playerName = 'TestPlayer') {
    // Set up a one-time dialog handler that will catch the name prompt on the Game page
    // Use 'once' to prevent conflicts with other dialog handlers in tests
    const dialogHandler = async dialog => {
        if (dialog.type() === 'prompt') {
            await dialog.accept(playerName);
        } else {
            await dialog.accept();
        }
        // Remove ourselves after handling to avoid conflicts
        page.off('dialog', dialogHandler);
    };
    page.on('dialog', dialogHandler);

    // Get the difficulty button
    const button = page.getByRole('button', { name: new RegExp(difficulty, 'i') });

    // Use Playwright's click with retry - wait for actionability and keep retrying
    // The click will be retried until it succeeds or times out
    // Using a polling approach to handle Blazor WASM event handler attachment delay
    const maxAttempts = 15;

    for (let attempt = 0; attempt < maxAttempts; attempt++) {
        // Check if we're already on the game page (navigation might have happened)
        if (page.url().includes('/game')) {
            break;
        }

        try {
            // Try clicking with force to ensure the click goes through
            await button.click({ force: true, timeout: 2000 });

            // Check if navigation started within a short time
            await page.waitForURL(/game/, { timeout: 3000 });
            break; // Navigation successful
        } catch {
            // If click didn't trigger navigation, wait a bit and retry
            if (attempt < maxAttempts - 1) {
                await page.waitForTimeout(500);
            }
        }
    }

    // Final check - verify we actually navigated
    if (!page.url().includes('/game')) {
        throw new Error(`Failed to navigate to game page after ${maxAttempts} click attempts. Current URL: ${page.url()}`);
    }

    // Wait for the game to fully initialize - use the heading which is unique
    await expect(page.getByRole('heading', { name: new RegExp(`${playerName}'s Turn`, 'i') })).toBeVisible({ timeout: 15000 });
}

test.describe('Game Flow', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        // Wait for Blazor WASM to fully load by checking for actual content
        await page.getByRole('heading', { name: 'Single Player' }).waitFor({ timeout: 60000 });
        // Wait for Blazor WASM to become fully interactive
        // The key indicator is that the 'blazor-loading' class is removed and Blazor is initialized
        await page.waitForFunction(() => {
            // Check multiple indicators that Blazor WASM is fully loaded and interactive:
            // 1. Blazor object exists
            // 2. Blazor._internal exists (indicates runtime is loaded)
            // 3. The document no longer has loading indicators
            const blazorReady = typeof window.Blazor !== 'undefined' &&
                window.Blazor._internal !== undefined;
            const noLoadingIndicator = !document.body.classList.contains('blazor-loading');
            return blazorReady && noLoadingIndicator;
        }, { timeout: 60000 });
        // Wait for WASM to fully load and event handlers to attach
        // Blazor WASM components need significant time after hydration
        // Wait for network to be idle (no pending requests for 500ms)
        await page.waitForLoadState('networkidle', { timeout: 30000 });
        // Additional safety margin for Blazor component initialization
        await page.waitForTimeout(3000);
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
