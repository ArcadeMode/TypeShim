import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

// Directory containing this script (safe on Windows)
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// wasm-lib-publish output is a sibling of the vitest folder
const mapPath = path.resolve(
  __dirname,
  '..',
  '..',
  'wasm-lib-publish',
  'wwwroot',
  '_framework',
  'dotnet.runtime.js.map'
);

const minimalSourceMap = JSON.stringify(
  { version: 3, sources: [], names: [], mappings: '' },
  null,
  2
) + '\n';

fs.mkdirSync(path.dirname(mapPath), { recursive: true });

// Write if missing (or tiny)
let write = true;
try {
  write = fs.statSync(mapPath).size < 10;
} catch {
  write = true;
}

if (write) {
  fs.writeFileSync(mapPath, minimalSourceMap, 'utf8');
  console.log(`[empty-map] wrote ${mapPath}`);
} else {
  console.log(`[empty-map] exists ${mapPath}`);
}