import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [
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
