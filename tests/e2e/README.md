# PoConnectFive End-to-End Tests

This directory contains Playwright-based end-to-end tests for the PoConnectFive application.

## Setup

1. **Install Node.js** (v18 or higher)

2. **Install Playwright and dependencies**:
   ```bash
   npm install
   npx playwright install
   ```

3. **Ensure the application is running**:
   ```bash
   dotnet run --project PoConnectFive.Server/PoConnectFive.Server.csproj
   ```

## Running Tests

### Run all tests
```bash
npm test
```

### Run tests in headed mode (see browser)
```bash
npm run test:headed
```

### Run tests in UI mode (interactive)
```bash
npm run test:ui
```

### Debug tests
```bash
npm run test:debug
```

### View test report
```bash
npm run test:report
```

### Run specific test file
```bash
npx playwright test tests/e2e/game-flow.spec.js
```

### Run tests in specific browser
```bash
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit
```

## Test Structure

- **game-flow.spec.js** - Tests for complete game flow, moves, navigation
- **ai-difficulty.spec.js** - Tests for AI difficulty selection and behavior
- **leaderboard.spec.js** - Tests for leaderboard display and navigation

## Configuration

Test configuration is in `playwright.config.js`:
- Base URL: `http://localhost:5000`
- Browsers: Chromium, Firefox, WebKit, Mobile Chrome, Mobile Safari
- Screenshots on failure
- Video recording on failure
- Traces on retry

## Writing New Tests

1. Create a new `.spec.js` file in `tests/e2e/`
2. Import Playwright test utilities:
   ```javascript
   const { test, expect } = require('@playwright/test');
   ```
3. Write tests using describe/test blocks:
   ```javascript
   test.describe('Feature Name', () => {
     test('should do something', async ({ page }) => {
       await page.goto('/');
       await expect(page.locator('text=Something')).toBeVisible();
     });
   });
   ```

## CI/CD Integration

Tests can be integrated into GitHub Actions workflow. See `.github/workflows/e2e-tests.yml` for CI configuration.

## Troubleshooting

### Tests fail to connect to application
- Ensure the application is running on `http://localhost:5000`
- Check if ports are available
- Verify `BaseAddress` in `appsettings.json`

### Browser not found
- Run `npx playwright install` to install browsers

### Tests timeout
- Increase timeout in `playwright.config.js`
- Check if application is responding slowly
- Use `--headed` mode to see what's happening

## Resources

- [Playwright Documentation](https://playwright.dev/docs/intro)
- [Playwright API](https://playwright.dev/docs/api/class-test)
- [Best Practices](https://playwright.dev/docs/best-practices)
