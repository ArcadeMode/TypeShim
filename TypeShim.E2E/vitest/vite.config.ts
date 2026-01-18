import { defineConfig } from 'vite';
import { e2eConfig } from './e2e.config';
import path from 'path';

export default defineConfig({
  plugins: [
    {
      name: 'strip-dotnet-sourcemap-urls',
      transform(code, id) {
        // Stripping sourceMappingURL directives avoids noisy ENOENT warnings.
        if (!id.includes(`${path.sep}e2e-wasm-app${path.sep}wwwroot${path.sep}_framework${path.sep}`)) {
          return null;
        }
        if (!/sourceMappingURL=/.test(code)) return null;
        const next = code.replace(/\n\/\/# sourceMappingURL=.*\s*$/g, '');
        return { code: next, map: null };
      },
    },
  ],
  root: '.',
  assetsInclude: ['**/*.wasm', '**/*.dat'],
  test: {
    root: '.',
    include: ['**/*.{test,spec}.ts', '**/*.{test,spec}.tsx'],
    environment: 'node',
    setupFiles: ['./vitest.setup.ts'],
    testTimeout: 10_000,
    hookTimeout: 10_000,
    browser: {
      enabled: e2eConfig.browserMode === true,
      name: "chromium", 
      provider: "playwright",
      headless: true,
    },
  },

  server: {
    fs: {
      // Permit serving files from the wasm app root.
      allow: [
        path.resolve(__dirname, '../e2e-wasm-app'),
        path.resolve(__dirname),
      ],
    },
  },

  
});
