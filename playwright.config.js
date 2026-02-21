const { defineConfig, devices } = require('@playwright/test');

const baseURL = 'http://127.0.0.1:5099';

module.exports = defineConfig({
  testDir: './e2e',
  timeout: 60 * 1000,
  expect: {
    timeout: 5000
  },
  fullyParallel: true,
  retries: process.env.CI ? 1 : 0,
  use: {
    baseURL,
    trace: 'retain-on-failure'
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] }
    }
  ],
  webServer: {
    command: 'dotnet run --project src/BuyAndCook.Web/BuyAndCook.Web.csproj --urls ' + baseURL,
    url: baseURL,
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000
  }
});
