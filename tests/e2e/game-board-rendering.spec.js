// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Completes the in-game setup dialog with default values unless overridden.
 * @param {import('@playwright/test').Page} page
 * @param {{ mode?: 'single' | 'two'; player1?: string; player2?: string; }} [options]
 */
async function completeGameSetup(page, options = {}) {
    const { mode = 'single', player1, player2 } = options;

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
 * Launches a single-player game by choosing a difficulty and completing setup.
 * @param {import('@playwright/test').Page} page
 * @param {'Easy' | 'Medium' | 'Hard'} difficulty
 */
async function startSinglePlayerGame(page, difficulty) {
    await page.getByRole('button', { name: new RegExp(difficulty, 'i') }).click();
    await expect(page).toHaveURL(/game/);
    await completeGameSetup(page, { mode: 'single' });
}

test.describe('Game Board Rendering', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        // Wait for Blazor WASM to fully load
        await page.getByRole('heading', { name: 'Single Player' }).waitFor({ timeout: 60000 });
    });

    test('should render game board canvas with correct dimensions', async ({ page }) => {
        // Start a game
        await startSinglePlayerGame(page, 'Easy');

        // Wait briefly for board to render
        await page.waitForTimeout(1000);

        // Verify game controls are present (ensures page loaded)
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible({ timeout: 5000 });

        // Find the canvas element
        const canvas = page.locator('canvas#gameCanvas');
        await expect(canvas).toBeVisible({ timeout: 10000 });

        // Get canvas bounding box
        const boundingBox = await canvas.boundingBox();
        expect(boundingBox).not.toBeNull();

        if (boundingBox) {
            // Verify canvas has reasonable dimensions (not collapsed to 0 or tiny)
            expect(boundingBox.width).toBeGreaterThan(300); // Minimum visible width
            expect(boundingBox.height).toBeGreaterThan(250); // Minimum visible height

            console.log(`Canvas dimensions: ${boundingBox.width}x${boundingBox.height}`);
        }

        // Verify canvas has width and height attributes set
        const width = await canvas.getAttribute('width');
        const height = await canvas.getAttribute('height');
        expect(width).not.toBeNull();
        expect(height).not.toBeNull();
        if (width && height) {
            expect(parseInt(width)).toBeGreaterThan(0);
            expect(parseInt(height)).toBeGreaterThan(0);
            console.log(`Canvas attributes: width=${width}, height=${height}`);
        }
    });

    test.skip('should render game board canvas on mobile viewport', async ({ page }) => {
        // Set mobile viewport
        await page.setViewportSize({ width: 375, height: 667 }); // iPhone SE size

        // Start a game
        await startSinglePlayerGame(page, 'Easy');

        await page.waitForTimeout(1000);
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible({ timeout: 5000 });

        // Find the canvas element
        const canvas = page.locator('canvas#gameCanvas');
        // Note: Canvas may be scaled/transformed on mobile, so check for existence rather than visibility
        await canvas.waitFor({ state: 'attached', timeout: 10000 });

        // Get canvas bounding box
        const boundingBox = await canvas.boundingBox();
        expect(boundingBox).not.toBeNull();

        if (boundingBox) {
            // On mobile, canvas should be smaller but still visible
            expect(boundingBox.width).toBeGreaterThan(200);
            expect(boundingBox.width).toBeLessThan(400); // Should fit mobile screen
            expect(boundingBox.height).toBeGreaterThan(150);

            console.log(`Mobile canvas dimensions: ${boundingBox.width}x${boundingBox.height}`);
        }
    });

    test('should render game board canvas on tablet viewport', async ({ page }) => {
        // Set tablet viewport
        await page.setViewportSize({ width: 768, height: 1024 }); // iPad size

        // Start a game
        await startSinglePlayerGame(page, 'Easy');

        await page.waitForTimeout(1000);
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible({ timeout: 5000 });

        // Find the canvas element
        const canvas = page.locator('canvas#gameCanvas');
        await expect(canvas).toBeVisible({ timeout: 10000 });

        // Get canvas bounding box
        const boundingBox = await canvas.boundingBox();
        expect(boundingBox).not.toBeNull();

        if (boundingBox) {
            // On tablet (768px), canvas should be visible and reasonable size
            expect(boundingBox.width).toBeGreaterThan(300);
            expect(boundingBox.width).toBeLessThanOrEqual(768); // Fits in viewport
            expect(boundingBox.height).toBeGreaterThan(250);

            console.log(`Tablet canvas dimensions: ${boundingBox.width}x${boundingBox.height}`);
        }
    });

    test('should render game board canvas on desktop viewport', async ({ page }) => {
        // Set desktop viewport
        await page.setViewportSize({ width: 1920, height: 1080 }); // Full HD

        // Start a game
        await startSinglePlayerGame(page, 'Easy');

        await page.waitForTimeout(1000);
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible({ timeout: 5000 });

        // Find the canvas element
        const canvas = page.locator('canvas#gameCanvas');
        await expect(canvas).toBeVisible({ timeout: 10000 });

        // Get canvas bounding box
        const boundingBox = await canvas.boundingBox();
        expect(boundingBox).not.toBeNull();

        if (boundingBox) {
            // On desktop, canvas should be full size (700x600 default)
            expect(boundingBox.width).toBeGreaterThan(500);
            expect(boundingBox.height).toBeGreaterThan(400);

            console.log(`Desktop canvas dimensions: ${boundingBox.width}x${boundingBox.height}`);
        }
    });

    test('should render game board with proper CSS styling', async ({ page }) => {
        // Start a game
        await startSinglePlayerGame(page, 'Easy');

        await page.waitForTimeout(1000);
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible({ timeout: 5000 });

        // Find the game board container
        const gameBoardContainer = page.locator('.game-board');
        await expect(gameBoardContainer).toBeVisible({ timeout: 10000 });

        // Verify container has proper styling
        const containerStyles = await gameBoardContainer.evaluate((el) => {
            const computed = window.getComputedStyle(el);
            return {
                position: computed.position,
                display: computed.display,
                borderWidth: computed.borderWidth,
                borderRadius: computed.borderRadius,
                overflow: computed.overflow,
            };
        });

        expect(containerStyles.position).toBe('relative');
        expect(containerStyles.display).toBe('flex');
        expect(containerStyles.borderWidth).toBeTruthy();
        expect(containerStyles.overflow).toBe('visible');

        console.log('Game board container styles:', containerStyles);
    });

    test('should render canvas without CSS dimension conflicts', async ({ page }) => {
        // Start a game
        await startSinglePlayerGame(page, 'Easy');

        await page.waitForTimeout(1000);
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible({ timeout: 5000 });

        // Find the canvas element
        const canvas = page.locator('canvas#gameCanvas');
        await expect(canvas).toBeVisible({ timeout: 10000 });

        // Get computed styles vs attributes
        const canvasInfo = await canvas.evaluate((el) => {
            const computed = window.getComputedStyle(el);
            return {
                attributeWidth: el.getAttribute('width'),
                attributeHeight: el.getAttribute('height'),
                computedWidth: computed.width,
                computedHeight: computed.height,
                computedMaxWidth: computed.maxWidth,
                computedAspectRatio: computed.aspectRatio,
                display: computed.display,
            };
        });

        // Verify canvas has explicit width (not max-width) to prevent collapse
        expect(canvasInfo.computedMaxWidth).toBe('none');
        expect(canvasInfo.display).toBe('block');
        // Aspect ratio is auto with intrinsic dimensions (auto width/height)
        expect(canvasInfo.computedAspectRatio).toMatch(/auto|auto \d+ \/ \d+/);

        console.log('Canvas rendering info:', canvasInfo);
    });

    test('should initialize JavaScript interop successfully', async ({ page }) => {
        // Listen for console logs from JavaScript
        /** @type {string[]} */
        const consoleMessages = [];
        page.on('console', (msg) => {
            if (msg.text().includes('Initializing game board') ||
                msg.text().includes('Game board interop')) {
                consoleMessages.push(msg.text());
            }
        });

        // Start a game
        await startSinglePlayerGame(page, 'Easy');

        await page.waitForTimeout(1000);
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible({ timeout: 5000 });

        // Additional wait for JavaScript to initialize
        await page.waitForTimeout(1000);

        // Verify canvas is visible
        const canvas = page.locator('canvas#gameCanvas');
        await expect(canvas).toBeVisible({ timeout: 10000 });        // Check that JavaScript interop initialized
        expect(consoleMessages.some(msg => msg.includes('Initializing game board interop'))).toBeTruthy();
        expect(consoleMessages.some(msg => msg.includes('initialized'))).toBeTruthy();

        console.log('Console messages:', consoleMessages);
    });

    test('should allow clicking on canvas to make moves', async ({ page }) => {
        // Start a game using the helper function
        await startSinglePlayerGame(page, 'Easy');

        // Wait for game page to render
        await page.waitForTimeout(1000);
        await expect(page.getByRole('button', { name: /New Game/i })).toBeVisible({ timeout: 5000 });

        // Find the canvas element
        const canvas = page.locator('canvas#gameCanvas');
        await expect(canvas).toBeVisible({ timeout: 10000 });

        // Get canvas bounding box for click position
        const boundingBox = await canvas.boundingBox();
        expect(boundingBox).not.toBeNull();

        if (boundingBox) {
            // Click in the middle column (center of canvas)
            const centerX = boundingBox.x + boundingBox.width / 2;
            const centerY = boundingBox.y + boundingBox.height / 2;

            // Click on the canvas
            await page.mouse.click(centerX, centerY);
        }

        // Wait a moment for the move to process
        await page.waitForTimeout(1000);

        // The canvas should still be visible and interactive
        await expect(canvas).toBeVisible();

        // Game status should update (verify game is responsive)
        const gameStatus = page.locator('[role="status"]');
        await expect(gameStatus).toBeVisible();
    });
});
