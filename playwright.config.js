const { defineConfig, devices } = require('@playwright/test');

const useStub = process.env.SILPO_API_STUB ? process.env.SILPO_API_STUB === 'true' : true;
const defaultBaseURL = 'http://127.0.0.1:5099';
const stubBaseURL = 'http://127.0.0.1:5100';
const baseURL = process.env.PLAYWRIGHT_BASE_URL || (useStub ? stubBaseURL : defaultBaseURL);
const reuseExistingServer = process.env.PLAYWRIGHT_REUSE_SERVER
  ? process.env.PLAYWRIGHT_REUSE_SERVER === 'true'
  : !process.env.CI;

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
    reuseExistingServer: reuseExistingServer && !useStub,
    timeout: 120 * 1000,
    env: {
      ASPNETCORE_ENVIRONMENT: 'Development',
      SilpoApi__UseStub: useStub ? 'true' : 'false'
    }
  }
});
