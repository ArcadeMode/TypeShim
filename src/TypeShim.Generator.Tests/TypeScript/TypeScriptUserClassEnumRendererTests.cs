using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassEnumRendererTests
{
    private static (ClassInfo cls, List<NamedTypeInfo> all) Build(string members)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public enum Color { Red, Green, Blue }
            [TSExport]
            public class C1
            {
            {{members}}
            }
        """.Replace("{{members}}", members));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> symbols = [.. symbolExtractor.ExtractAllExportedSymbols()];
        InteropTypeInfoCache cache = new();
        List<NamedTypeInfo> all = [.. symbols.Select(s => new NamedTypeInfoBuilder(s, cache).Build()).OfType<NamedTypeInfo>()];
        ClassInfo cls = all.OfType<ClassInfo>().First(c => c.Name == "C1");
        return (cls, all);
    }

    private static string RenderProxy(string members)
    {
        (ClassInfo cls, List<NamedTypeInfo> all) = Build(members);
        RenderContext ctx = new(cls, all, RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(ctx).Render();
        return ctx.ToString();
    }

    private static string RenderNamespace(string members)
    {
        (ClassInfo cls, List<NamedTypeInfo> all) = Build(members);
        RenderContext ctx = new(cls, all, RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(ctx).Render();
        return ctx.ToString();
    }

    private static string RenderAssemblyExports(string members)
    {
        (ClassInfo cls, List<NamedTypeInfo> all) = Build(members);
        List<ClassInfo> classes = [.. all.OfType<ClassInfo>()];
        ModuleHierarchyInfo hierarchy = ModuleHierarchyInfo.FromClasses(classes);
        RenderContext ctx = new(null, all, RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchy, ctx).Render();
        return ctx.ToString();
    }

    [Test]
    public void ProxyMethod_EnumParamAndReturn_RenderEnumNameAndPassValueThrough()
    {
        string ts = RenderProxy("    public Color Echo(Color c) => c;");

        AssertEx.EqualOrDiff(ts, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public Echo(c: Color): Color {
    return TypeShimConfig.exports.N1.C1Interop.Echo(this.instance, c);
  }
}

""");
    }

    [Test]
    public void ProxyProperty_EnumGetterAndSetter_RenderEnumNameAndPassValueThrough()
    {
        string ts = RenderProxy("    public Color P1 { get; set; }");

        AssertEx.EqualOrDiff(ts, """
export class C1 extends ProxyBase {
  /**
   * @param initializer - Object with member-initializers
   */
  constructor(initializer: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...initializer }));
  }

  public get P1(): Color {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }

  public set P1(value: Color) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value);
  }
}

""");
    }

    [Test]
    public void Namespace_EnumProperty_UsesEnumNameInShapesAndIdentityMaterialize()
    {
        string ts = RenderNamespace("    public Color P1 { get; set; }");

        AssertEx.EqualOrDiff(ts, """
export namespace C1 {
  export interface Initializer {
    P1: Color;
  }
  export interface Snapshot {
    P1: Color;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1,
    };
  }
}

""");
    }

    [Test]
    public void ProxyMethod_EnumCompositeTypes_ComposeAsPlainArrayNullableAndPromise()
    {
        string ts = RenderProxy("""
                public Color[] Arr(Color[] c) => c;
                public Color? Nul(Color? c) => c;
                public Task<Color> Tsk() => Task.FromResult(Color.Red);
        """);

        AssertEx.EqualOrDiff(ts, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public Arr(c: Array<Color>): Array<Color> {
    return TypeShimConfig.exports.N1.C1Interop.Arr(this.instance, c);
  }

  public Nul(c: Color | null): Color | null {
    return TypeShimConfig.exports.N1.C1Interop.Nul(this.instance, c);
  }

  public async Tsk(): Promise<Color> {
    return TypeShimConfig.exports.N1.C1Interop.Tsk(this.instance);
  }
}

""");
    }

    [Test]
    public void AssemblyExports_EnumParamAndReturn_RenderAsNumberOnInteropBoundary()
    {
        string ts = RenderAssemblyExports("    public Color Echo(Color c) => c;");

        AssertEx.EqualOrDiff(ts, """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      ctor(): ManagedObject;
      Echo(instance: ManagedObject, c: number): number;
    };
  };
}

""");
    }
}
