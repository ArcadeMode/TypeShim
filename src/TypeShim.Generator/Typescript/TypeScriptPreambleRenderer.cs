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

  static async initialize(runtimeInfo: any) {
    if (TypeShimConfig._exports){
      throw new Error("TypeShim has already been initialized.");
    }

    runtimeInfo.setModuleImports("@typeshim", { 
      unwrapProperty: (obj: any, propertyName: string) => obj[propertyName],
    });
    TypeShimConfig._exports = await runtimeInfo.getAssemblyExports(runtimeInfo.getConfig().mainAssemblyName);
  }
}

export const TypeShimInitializer = { initialize: TypeShimConfig.initialize };

const proxyMap = new WeakMap<ManagedObject, ProxyBase>();
abstract class ProxyBase {
  instance: ManagedObject;
  constructor(instance: ManagedObject) {
    this.instance = instance;
    proxyMap.set(instance, this);
  }

  static fromHandle<T extends ProxyBase>(ctor: { prototype: T }, handle: ManagedObject): T {
    if (proxyMap.has(handle)) {
      return proxyMap.get(handle) as T;
    }
    const obj = Object.create(ctor.prototype) as T;
    obj.instance = handle;
    proxyMap.set(handle, obj);
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

interface IMemoryView<TArray> extends IDisposable {
    /**
     * copies elements from provided source to the wasm memory.
     * target has to have the elements of the same type as the underlying C# array.
     * same as TypedArray.set()
     */
    set(source: TArray, targetOffset?: number): void;
    /**
     * copies elements from wasm memory to provided target.
     * target has to have the elements of the same type as the underlying C# array.
     */
    copyTo(target: TArray, sourceOffset?: number): void;
    /**
     * same as TypedArray.slice()
     */
    slice(start?: number, end?: number): TArray;

    get length(): number;
    get byteLength(): number;
}

declare const __instantiationGuard: unique symbol; // deliberately not exported
type DoNotInstantiateGuard<Msg extends string> = { readonly [__instantiationGuard]: Msg };

export type Span<TArray> = IMemoryView<TArray> & DoNotInstantiateGuard<"Span cannot be instantiated from JS">
export type ArraySegment<TArray> = IMemoryView<TArray> & DoNotInstantiateGuard<"ArraySegment cannot be instantiated from JS">
""";
}
