using System.Text;

namespace TypeShim.Generator.Typescript;

internal class TypescriptConfigRenderer(RenderContext ctx)
{
    internal void Render()
    {
        ctx.AppendLine(TypeShimConfigClass);
    }

    private const string TypeShimConfigClass = """
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
    options.setModuleImports("@typeshim", { unwrap: (obj: any) => obj });
    TypeShimConfig._exports = options.assemblyExports;
  }
}

export const TypeShimInitializer = { initialize: TypeShimConfig.initialize };

abstract class ProxyBase {
  instance: object;
  constructor(instance: object) {
    this.instance = instance;
  }

  static fromHandle<T extends ProxyBase>(ctor: { prototype: T }, handle: object): T {
    const obj = Object.create(ctor.prototype) as T;
    obj.instance = handle;
    return obj;
  }
}

""";
}
