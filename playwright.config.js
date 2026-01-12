// @ts-check
const { defineConfig, devices } = require('@playwright/test');

/**
 * @see https://playwright.dev/docs/test-configuration
 */
module.exports = defineConfig({
    testDir: './tests/e2e',

    /* Global timeout for each test - increased for Blazor WASM init */
    timeout: 90000,

    /* Run tests in files in parallel */
    fullyParallel: true,

    /* Fail the build on CI if you accidentally left test.only in the source code. */
    forbidOnly: !!process.env.CI,

    /* Retry on CI only */
    retries: process.env.CI ? 2 : 1,

    /* Opt out of parallel tests on CI. */
    workers: process.env.CI ? 1 : undefined,

    /* Reporter to use. See https://playwright.dev/docs/test-reporters */
    reporter: [
        ['html'],
        ['list']
    ],

    /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
    use: {
        /* Base URL to use in actions like `await page.goto('/')`. */
        baseURL: 'http://localhost:5000',

        /* Collect trace on failure for triage */
        trace: 'retain-on-failure',

        /* Screenshot on failure */
        screenshot: 'only-on-failure',

        /* Video on failure */
        video: 'retain-on-failure',

        /* Increase navigation timeout for Blazor WASM initialization */
        navigationTimeout: 60000,

        /* Increase action timeout */
        actionTimeout: 15000,

        /* Headless mode */
        headless: true,
    },

    /* Configure projects for chromium desktop and mobile only */
    projects: [
        {
            name: 'chromium',
            use: { ...devices['Desktop Chrome'] },
        },
        {
            name: 'Mobile Chrome',
            use: { ...devices['Pixel 5'] },
        },
    ],

    /* Run your local dev server before starting the tests */
    webServer: {
        command: 'dotnet run --project src/Po.ConnectFive.Api/Po.ConnectFive.Api.csproj --urls http://localhost:5000',
        url: 'http://localhost:5000/api/health',
        reuseExistingServer: true,
        timeout: 180 * 1000,
        stdout: 'pipe',
        stderr: 'pipe',
    },
});
