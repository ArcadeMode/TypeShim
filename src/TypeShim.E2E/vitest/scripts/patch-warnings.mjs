import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const dir = path.dirname(fileURLToPath(import.meta.url));
const dotnetRuntimeFilePath = path.resolve(dir, '..', '..', 'wasm-lib-publish', 'wwwroot', '_framework', 'dotnet.runtime.js');
const dotneFilePath = path.resolve(dir, '..', '..', 'wasm-lib-publish', 'wwwroot', '_framework', 'dotnet.js');
createEmptyMapFile(`${dotnetRuntimeFilePath}.map`);
insertViteIgnores(dotnetRuntimeFilePath);
insertViteIgnores(dotneFilePath);

function createEmptyMapFile(mapPath) {
    const json = '{"version":3,"sources":[],"names":[],"mappings":""}\n';
    fs.mkdirSync(path.dirname(mapPath), { recursive: true });
    let shouldWrite = true;
    try { shouldWrite = fs.statSync(mapPath).size < 10; } catch {}

    if (shouldWrite) fs.writeFileSync(mapPath, json, 'utf8');
}

function insertViteIgnores(file){ 
    const abs=path.resolve(file); 
    const src=fs.readFileSync(abs,'utf8'); 
    const out=src.replace(/\/\*! webpackIgnore: true \*\//g,'/* @vite-ignore */')
               .replace(/await\s+import\(\s*(?!\/\*)/g, 'await import(/* @vite-ignore */ ');
               // dotnet missed some imports, we do lookahead to see missing comment in import, then add where missing
    if(out!==src) fs.writeFileSync(abs,out,'utf8'); 
    console.log(`[replace] ${out===src?'no changes':'updated'} ${abs}`); 
}