import { defineConfig } from 'vite';

export default defineConfig({
  plugins: [
  ],
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
      enabled: isBrowserMode(),
      name: 'chromium',
      provider: 'playwright',
      headless: true,
    },
    reporters: [
      'default',
      ['junit', {
        suiteName: isBrowserMode() ? 'E2E (Browser)' : 'E2E (Node)'
      }]
    ] as any,
    outputFile: (
      { junit: isBrowserMode() ? '../e2e-report-browser.xml' : '../e2e-report-node.xml' }
    ) as any
  }
});

export function isBrowserMode(): boolean {
  return (process.env.BROWSER_MODE ?? '').toLowerCase() === 'true';
}