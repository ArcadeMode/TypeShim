// Importing the Blazor boot script attaches the Blazor JS API to `window.Blazor`.
// Because webpack bundles this module into our entry chunk, its relative runtime
// imports (e.g. `./dotnet.js`) would otherwise resolve against the bundle's own
// URL (`/assets/…`) instead of the `_framework/` directory the plugin serves.
import '_framework/blazor.webassembly.js';

// One webpack-managed stylesheet, assembled from the Blazor Library's own CSS.
import './styles.css';

// The Weather page fetches this at runtime via HttpClient. Importing it pulls the
// file into webpack's graph so it is emitted at a stable `/sample-data/weather.json`.
import './sample-data/weather.json';

// favicon.png is a .NET static web asset; importing it (bare specifier, resolved by
// unplugin-dotnet-wasm) emits it so the <link rel="icon"> in index.html resolves.
import 'favicon.png';

async function boot(): Promise<void> {
  // The entry is loaded as a module script, so Blazor does not auto-start; we start
  // it ourselves. In a production/dist build the plugin resolves the runtime to
  // physical paths (no `webpackIgnore`), so webpack bundles `dotnet.js` and Blazor's
  // default `import("./dotnet.js")` path resolves to the bundled module.
  await window.Blazor.start();
  console.log('[blazor] started');
}

boot().catch((err) => console.error('[blazor] failed to start', err));
