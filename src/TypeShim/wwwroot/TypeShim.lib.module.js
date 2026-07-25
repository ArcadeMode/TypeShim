const moduleName = "@typeshim";
const STATE_KEY = Symbol.for(moduleName);

export async function onRuntimeReady({ setModuleImports, getAssemblyExports, getConfig }) {
    const state = globalThis[STATE_KEY] ??= {};

    if (state.exports) {
        console.warn("TypeShim is already configured for another dotnet runtime, this will be an issue if you are trying to use multiple runtimes simultaneously.");
    }
    console.log("TypeShim is configuring module imports for dotnet runtime.");
    setModuleImports(moduleName, {
        unwrapProperty: (obj, propertyName) => obj[propertyName],
    });
    state.exports = await getAssemblyExports(getConfig().mainAssemblyName);
    console.log("TypeShim is configured for dotnet runtime.");
}
