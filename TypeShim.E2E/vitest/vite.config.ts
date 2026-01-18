import { defineConfig } from 'vite';
import path from 'path';

export default defineConfig({
  plugins: [
    {
      name: 'strip-dotnet-sourcemap-urls',
      transform(code, id) {
        // The dotnet runtime JS files in wwwroot/_framework often reference *.map files
        // that are not shipped. Stripping the directive avoids noisy ENOENT warnings.
        if (!id.includes(`${path.sep}e2e-wasm-app${path.sep}wwwroot${path.sep}_framework${path.sep}`)) {
          return null;
        }
        if (!/sourceMappingURL=/.test(code)) return null;
        const next = code.replace(/\n\/\/# sourceMappingURL=.*\s*$/g, '');
        return { code: next, map: null };
      },
    },
  ],
  // Keep this project rooted here.
  root: '.',


  // Allow Vite to include the wasm/dat assets the .NET runtime loads.
  assetsInclude: ['**/*.wasm', '**/*.dat'],

  // Vitest runs in Node, but we'll use a DOM-like environment for dotnet.js.
  test: {
    // Keep test discovery rooted at this Vitest project folder.
    root: '.',
    include: ['**/*.{test,spec}.ts', '**/*.{test,spec}.tsx'],
    environment: 'node',
    setupFiles: ['./vitest.setup.ts'],
    testTimeout: 10_000,
    hookTimeout: 10_000,
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
