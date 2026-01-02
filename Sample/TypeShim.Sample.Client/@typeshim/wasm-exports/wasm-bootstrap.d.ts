export type DotnetWasmRuntimeInfo = {
    assemblyExports: AssemblyExports;
    setModuleImports: (moduleName: string, imports: object) => void;
}
export function createWasmRuntime(args?: string | undefined): Promise<DotnetWasmRuntimeInfo>;