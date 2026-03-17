const { defineConfig, devices } = require('@playwright/test');
const fs = require('fs');
const os = require('os');
const path = require('path');

const useStub = process.env.SILPO_API_STUB === 'true';
const workers = process.env.PLAYWRIGHT_WORKERS
  ? Number(process.env.PLAYWRIGHT_WORKERS)
  : useStub
    ? undefined
    : 1;
const isolateDb = process.env.PLAYWRIGHT_DB_ISOLATED
  ? process.env.PLAYWRIGHT_DB_ISOLATED === 'true'
  : true;
const defaultBaseURL = isolateDb ? 'http://127.0.0.1:5101' : 'http://127.0.0.1:5099';
const stubBaseURL = 'http://127.0.0.1:5100';
const baseURL = process.env.PLAYWRIGHT_BASE_URL || (useStub ? stubBaseURL : defaultBaseURL);
const reuseExistingServer = process.env.PLAYWRIGHT_REUSE_SERVER
  ? process.env.PLAYWRIGHT_REUSE_SERVER === 'true'
  : !process.env.CI;
const defaultDbPath = path.join(os.tmpdir(), 'buyandcook-e2e', 'buyandcook-e2e.db');
const e2eDbPath = process.env.BUYANDCOOK_DB_PATH || defaultDbPath;
const dbArg = `Database:Path=${e2eDbPath}`;
const stubArg = `SilpoApi:UseStub=${useStub ? 'true' : 'false'}`;
const webServerArgs = `-- ${dbArg} ${stubArg}`;

if (isolateDb && fs.existsSync(e2eDbPath)) {
  fs.rmSync(e2eDbPath);
}

module.exports = defineConfig({
  testDir: './e2e',
  timeout: 60 * 1000,
  expect: {
    timeout: 5000
  },
  fullyParallel: useStub,
  workers,
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
    command: 'dotnet run --project src/BuyAndCook.Web/BuyAndCook.Web.csproj --urls ' + baseURL + ' ' + webServerArgs,
    url: baseURL,
    reuseExistingServer: reuseExistingServer && !useStub && !isolateDb,
    timeout: 120 * 1000,
    env: {
      ASPNETCORE_ENVIRONMENT: 'Development',
      SilpoApi__UseStub: useStub ? 'true' : 'false',
      Database__Path: e2eDbPath,
      BUYANDCOOK_DB_PATH: e2eDbPath
    }
  }
});
