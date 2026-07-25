const moduleName = "@typeshim";
const STATE_KEY = Symbol.for(moduleName);

export async function onRuntimeReady({ setModuleImports, getAssemblyExports, getConfig }) {
    const state = globalThis[STATE_KEY] ?? {};
    if (state.exports) {
        console.warn("TypeShim is already configured for another dotnet runtime, this will be an issue if you are trying to use multiple runtimes simultaneously.");
    }

    setModuleImports(moduleName, {
        unwrapProperty: (obj, propertyName) => obj[propertyName],
    });

    state.exports = await getAssemblyExports(getConfig().mainAssemblyName);
    globalThis[STATE_KEY] = state;
}
