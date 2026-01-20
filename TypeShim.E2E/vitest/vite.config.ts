import { defineConfig } from 'vite';

const isBrowserMode = ((process.env.VITE_BROWSER_MODE ?? '').toLowerCase() === 'true');
const isCI = ((process.env.CI ?? '').toLowerCase() === 'true');

export default defineConfig({
  define: { __BROWSER_MODE__: isBrowserMode, __IS_CI__: isCI },
  root: '.',
  assetsInclude: ['**/*.wasm', '**/*.dat'],
  test: {
    root: '.',
    include: ['**/*.{test,spec}.ts', '**/*.{test,spec}.tsx'],
    environment: 'node',
    setupFiles: ['./src/setup/setup.ts'],
    testTimeout: 10_000,
    hookTimeout: 10_000,
    browser: {
      enabled: isBrowserMode,
      name: 'chromium',
      provider: 'playwright',
      headless: true,
      screenshotFailures: false
    },
    reporters: isCI ? [['junit', { suiteName: isBrowserMode ? 'E2E (Browser)' : 'E2E (Node)' }]] : ['default'],
    outputFile: {
       junit: isBrowserMode ? '../e2e-report-browser.xml' : '../e2e-report-node.xml' 
    }
  }
});
