using System.Text;

namespace TypeShim.Generator.Typescript;

internal class TypeScriptPreambleRenderer(RenderContext ctx)
{
    internal void Render()
    {
        ctx.AppendLine(Preamble);
    }

    private const string Preamble = """
class TypeShimConfig {
  private static _exports: AssemblyExports | null = null;

  static get exports() {
    if (!TypeShimConfig._exports) {
      throw new Error("TypeShim has not been initialized.");
    }
    return TypeShimConfig._exports;
  }

  static initialize(options: { assemblyExports: AssemblyExports, setModuleImports: (scriptName: string, imports: object) => void }) {
    if (TypeShimConfig._exports){
      throw new Error("TypeShim has already been initialized.");
    }
    options.setModuleImports("@typeshim", { 
      unwrap: (obj: any) => obj, 
      unwrapProperty: (obj: any, propertyName: string) => obj[propertyName],
      unwrapCharPromise: (promise: Promise<any>) => promise.then(c => c.charCodeAt(0))
    });
    TypeShimConfig._exports = options.assemblyExports;
  }
}

export const TypeShimInitializer = { initialize: TypeShimConfig.initialize };

abstract class ProxyBase {
  instance: ManagedObject;
  constructor(instance: ManagedObject) {
    this.instance = instance;
  }

  static fromHandle<T extends ProxyBase>(ctor: { prototype: T }, handle: ManagedObject): T {
    const obj = Object.create(ctor.prototype) as T;
    obj.instance = handle;
    return obj;
  }
}

export interface IDisposable {
    dispose(): void;
    get isDisposed(): boolean;
}

export interface ManagedObject extends IDisposable {
    toString (): string;
}

export interface ManagedError extends IDisposable {
    get stack(): any;
    getSuperStack(): any; 
    getManageStack(): any;
}

""";
}
