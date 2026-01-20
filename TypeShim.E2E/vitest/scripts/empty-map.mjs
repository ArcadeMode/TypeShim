import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const dir = path.dirname(fileURLToPath(import.meta.url));
const mapPath = path.resolve(dir, '..', '..', 'wasm-lib-publish', 'wwwroot', '_framework', 'dotnet.runtime.js.map');
const json = '{"version":3,"sources":[],"names":[],"mappings":""}\n';

fs.mkdirSync(path.dirname(mapPath), { recursive: true });

let shouldWrite = true;
try { shouldWrite = fs.statSync(mapPath).size < 10; } catch {}

if (shouldWrite) fs.writeFileSync(mapPath, json, 'utf8');