import { defineConfig } from 'vite';
import { e2eConfig } from './e2e.config';
import path from 'path';

export default defineConfig({
  plugins: [
  ],
  root: '.',
  assetsInclude: ['**/*.wasm', '**/*.dat'],
  test: {
    root: '.',
    include: ['**/*.{test,spec}.ts', '**/*.{test,spec}.tsx'],
    environment: 'node',
    setupFiles: ['./src/setup.ts'],
    testTimeout: 10_000,
    hookTimeout: 10_000,
    browser: {
      enabled: e2eConfig.browserMode === true,
      name: "chromium", 
      provider: "playwright",
      headless: true,
    },
  },

  // server: {
  //   fs: {
  //     // Permit serving files from the wasm app root.
  //     allow: [
  //       path.resolve(__dirname, '../e2e-wasm-app'),
  //       path.resolve(__dirname),
  //     ],
  //   },
  // },

  
});
