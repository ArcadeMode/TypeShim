## TLDR;
_In the webpack-vanilla=blazor directory:_ start the vite dev server and the ASP.NET backend together with
```
npm install
npm run build
npm start
```

The app should be available on [http://localhost:5080](http://localhost:5080)

# .NET Blazor WebAssembly + Webpack with TypeShim

This sample bundles a Blazor app into a JS module with webpack, while TypeShim provides the interop boundary code. It demonstrates an inversion of the standard blazor webassembly deployment where Blazor is shipped with extra JS modules, here Blazor is shipped as part of a JS module, enabling the integration with the JS ecosystem.

To demonstrate the integration with JS, the Blazor sample 'Counter' page has been extended to show confetti (`js-confetti`) when a counter changes. TypeShim is employed to make a simple cross language event bridge. 