import { readFile } from 'node:fs/promises';
import path from 'node:path';

// utility to handle fetch requests when running in node so wasm/dat files can be loaded from disk

type FetchInput = string | URL | Request;

function toUrlString(input: FetchInput): string {
  if (typeof input === 'string') return input;
  if (input instanceof URL) return input.toString();
  return input.url;
}

function contentTypeForFile(filePath: string): string {
  const ext = path.extname(filePath).toLowerCase();
  switch (ext) {
    case '.wasm':
      return 'application/wasm';
    case '.dat':
      return 'application/octet-stream';
    case '.json':
      return 'application/json';
    case '.js':
      return 'application/javascript';
    case '.mjs':
      return 'application/javascript';
    default:
      return 'application/octet-stream';
  }
}

function tryMapToFilePath(urlString: string): string | undefined {
  let pathname = urlString;
  if (/^https?:\/\//i.test(urlString)) {
    try {
      pathname = new URL(urlString).pathname;
    } catch { }
  }
  // Vite filesystem URLs look like: /@fs/C:/path/to/file
  if (pathname.startsWith('/@fs/')) {
    let fsPath = pathname.slice('/@fs/'.length);
    fsPath = fsPath.replace(/\//g, path.sep);
    return path.normalize(fsPath);
  }
  // When dotnet runtime uses its normal URLs, they look like /_framework/<asset>
  if (pathname.startsWith('/_framework/')) {
    const wasmWwwroot = path.resolve(__dirname, '../e2e-wasm-app/wwwroot');
    const rel = pathname.replace(/^\//, '');
    return path.join(wasmWwwroot, rel);
  }
  return undefined;
}

export function serveFetchRequestsFromDisk(): void {
  const originalFetch = globalThis.fetch?.bind(globalThis);
  if (!originalFetch) {
    throw new Error('globalThis.fetch is not available; Vitest must run on a Node version with fetch');
  }

  globalThis.fetch = (async (input: FetchInput, init?: RequestInit): Promise<Response> => {
    const urlString = toUrlString(input);
    const mappedPath = tryMapToFilePath(urlString);

    if (!mappedPath) {
      return originalFetch(input as any, init);
    }

    const body = await readFile(mappedPath);
    return new Response(body, {
      status: 200,
      headers: {
        'content-type': contentTypeForFile(mappedPath),
      },
    });
  }) as any;
}
