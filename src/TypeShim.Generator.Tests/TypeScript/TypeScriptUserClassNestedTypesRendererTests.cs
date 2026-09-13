using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassNestedTypesRendererTests
{
    private const string Source = """
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class Outer
        {
            private Outer() {}
            public static Inner Make(Kind k) => null;

            public enum Kind { A, B }

            public class Inner
            {
                private Inner() {}
                public static int Ping() => 1;
            }
        }

        [TSExport]
        public class Consumer
        {
            private Consumer() {}
            public static Outer.Inner Direct() => null;
            public static Outer.Inner[] Arr() => null;
            public static Outer.Inner? Nily() => null;
            public static Task<Outer.Inner> Asy() => null;
            public static Action<Outer.Inner> Del() => null;
            public static Outer.Kind EnumRef() => Outer.Kind.A;
        }
        """;

    private static (RenderContext ctx, ClassInfo target) Build(string className)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(Source);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exported = [.. symbolExtractor.ExtractAllExportedSymbols()];
        InteropTypeInfoCache typeCache = new();
        List<NamedTypeInfo> named = [.. exported.Select(s => new NamedTypeInfoBuilder(s, typeCache).Build()).OfType<NamedTypeInfo>()];
        ClassInfo target = named.OfType<ClassInfo>().First(c => c.Name == className);
        return (new RenderContext(target, named, RenderOptions.TypeScript), target);
    }

    [Test]
    public void UserClass_WithNestedClassAndEnum_RendersProxyAndNestedNamespace()
    {
        (RenderContext ctx, _) = Build("Outer");
        new TypescriptUserClassProxyRenderer(ctx).Render();
        new TypeScriptUserClassNamespaceRenderer(ctx).Render();

        AssertEx.EqualOrDiff(ctx.ToString(), """
export class Outer extends ProxyBase {
  private constructor() { super(undefined!); }

  public static Make(k: Outer.Kind): Outer.Inner {
    const res = TypeShimConfig.exports.N1.OuterInterop.Make(k);
    return ProxyBase.fromHandle(Outer.Inner, res);
  }
}
export namespace Outer {
  export enum Kind {
    A = 0,
    B = 1,
  }
  export class Inner extends ProxyBase {
    private constructor() { super(undefined!); }

    public static Ping(): number {
      return TypeShimConfig.exports.N1.OuterInterop.InnerInterop.Ping();
    }
  }
}

""");
    }

    [Test]
    public void UserClass_ReferencingNestedTypesOfAnotherClass_UsesDottedReferences()
    {
        (RenderContext ctx, _) = Build("Consumer");
        new TypescriptUserClassProxyRenderer(ctx).Render();

        AssertEx.EqualOrDiff(ctx.ToString(), """
export class Consumer extends ProxyBase {
  private constructor() { super(undefined!); }

  public static Direct(): Outer.Inner {
    const res = TypeShimConfig.exports.N1.ConsumerInterop.Direct();
    return ProxyBase.fromHandle(Outer.Inner, res);
  }

  public static Arr(): Array<Outer.Inner> {
    const res = TypeShimConfig.exports.N1.ConsumerInterop.Arr();
    return res.map(e => ProxyBase.fromHandle(Outer.Inner, e));
  }

  public static Nily(): Outer.Inner | null {
    const res = TypeShimConfig.exports.N1.ConsumerInterop.Nily();
    return res ? ProxyBase.fromHandle(Outer.Inner, res) : null;
  }

  public static async Asy(): Promise<Outer.Inner> {
    const res = TypeShimConfig.exports.N1.ConsumerInterop.Asy();
    return res.then(e => ProxyBase.fromHandle(Outer.Inner, e));
  }

  public static Del(): (arg0: Outer.Inner) => void {
    const res = TypeShimConfig.exports.N1.ConsumerInterop.Del();
    return (arg0: Outer.Inner) => res(arg0.instance);
  }

  public static EnumRef(): Outer.Kind {
    return TypeShimConfig.exports.N1.ConsumerInterop.EnumRef();
  }
}

""");
    }
}
