using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassProxyRendererTests_OptionalConstructorParameters
{
    private static string Render(string classDeclaration)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText($$"""
            using System;
            namespace N1;
            [TSExport]
            {{classDeclaration}}
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses[0], typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();
        return renderContext.ToString();
    }

    [Test]
    public void OptionalCtorParam_WithAllNullableInitializer_RendersOptionalInitializer()
    {
        string output = Render("""
            public class C1
            {
                public C1(int count = 5) {}
                public string? P1 { get; set; }
            }
        """);

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  /**
   * @param initializer - Object with member-initializers
   */
  constructor(count: number = 5, initializer?: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor(count, { ...initializer }));
  }

  public get P1(): string | null {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }

  public set P1(value: string | null) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value);
  }
}

""");
    }

    [Test]
    public void OptionalCtorParam_WithNonNullableInitializerMember_Throws()
    {
        Assert.Throws<NotSupportedOptionalParameterException>(() => Render("""
            public class C1
            {
                public C1(int count = 5) {}
                public string P1 { get; set; }
            }
        """));
    }

    [Test]
    public void OptionalCtorParam_WithNoInitializer_RendersDefaultOnly()
    {
        string output = Render("""
            public class C1
            {
                public C1(int count = 5) {}
                public string P1 { get; }
            }
        """);

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor(count: number = 5) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor(count));
  }

  public get P1(): string {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }
}

""");
    }

    [Test]
    public void OptionalCtorParam_WithRequiredProperty_Throws()
    {
        Assert.Throws<NotSupportedOptionalParameterException>(() => Render("""
            public class C1
            {
                public C1(int count = 5) {}
                public required string P1 { get; set; }
            }
        """));
    }

    [Test]
    public void OptionalCtorParam_WithNonNullableCharMember_Throws()
    {
        Assert.Throws<NotSupportedOptionalParameterException>(() => Render("""
            public class C1
            {
                public C1(int count = 5) {}
                public char P1 { get; set; }
            }
        """));
    }

    [Test]
    public void RequiredCtorParam_WithRequiredProperty_StillRendersRequiredInitializer()
    {
        string output = Render("""
            public class C1
            {
                public C1(int count) {}
                public required string P1 { get; set; }
            }
        """);

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  /**
   * @param initializer - Object with member-initializers
   */
  constructor(count: number, initializer: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor(count, { ...initializer }));
  }

  public get P1(): string {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }

  public set P1(value: string) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value);
  }
}

""");
    }
}
