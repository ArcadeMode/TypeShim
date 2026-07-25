import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import DotnetAssets from 'unplugin-dotnet-wasm/vite';

export default defineConfig({
  plugins: [
    DotnetAssets({
      projectName: 'Library',
      projectRoot: '../Library',
      configuration: 'Debug',
      targetFramework: 'net10.0',
      isPublish: false,
    }),
    react()
  ],
  assetsInclude: ['**/*.dat', '**/*.wasm', '**/*.pdb'],
  build: {
    target: 'es2020',
    outDir: './dist',
    assetsDir: 'assets',
    rollupOptions: {
      external: ['webcil', '/_framework/dotnet.js'],
      input: 'index.html',
      output: {
        entryFileNames: 'assets/[name].js',
        chunkFileNames: 'assets/[name].js',
        assetFileNames: 'assets/[name][extname]'
      }
    }
  }
});
