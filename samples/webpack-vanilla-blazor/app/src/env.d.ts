/**
 * Ambient declaration for the Blazor WebAssembly boot module emitted by the
 * .NET SDK under `_framework/` when `WasmBundlerFriendlyBootConfig=true`.
 * `unplugin-dotnet-wasm` resolves this virtual specifier to the physical file.
 */
declare module '_framework/blazor.webassembly.js';

/**
 * CSS is handled by webpack loaders, not TypeScript. This ambient declaration
 * gives `import './styles.css'` a type so the editor/`tsc` stop reporting the
 * import as an unresolved module.
 */
declare module '*.css';

interface BlazorStartOptions {
  loadBootResource?: (
    type: string,
    name: string,
    defaultUri: string,
    integrity: string,
  ) => string | Promise<Response> | null | undefined;
  [key: string]: unknown;
}

interface BlazorGlobal {
  start(options?: BlazorStartOptions): Promise<void>;
}

interface Window {
  Blazor: BlazorGlobal;
}
