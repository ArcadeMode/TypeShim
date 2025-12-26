using System.Text;

namespace TypeShim.Generator.Typescript;

internal class TypescriptConfigRenderer()
{
    private readonly StringBuilder sb = new();

    internal string Render()
    {
        sb.AppendLine(TypeShimConfigClass);
        return sb.ToString();
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

  static initialize(options: { exports: AssemblyExports }) {
    if (TypeShimConfig._exports){
      throw new Error("TypeShim has already been initialized.");
    }
    TypeShimConfig._exports = options.exports;
  }
}

export const TypeShimInitializer = { initialize: TypeShimConfig.initialize };
""";
}
