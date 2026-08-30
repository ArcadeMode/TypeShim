using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassProxyRendererTests_OptionalParameters
{
    private static string Render(string classBody)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText($$"""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
            {{classBody}}
            }
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
    public void OptionalPrimitiveParameters_RenderDefaults()
    {
        string output = Render("    public void M1(int count = 5, bool flag = true, double ratio = 1.5, string label = \"abc\") {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(count: number = 5, flag: boolean = true, ratio: number = 1.5, label: string = "abc"): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, count, flag, ratio, label);
  }
}

""");
    }

    [Test]
    public void OptionalParameterAfterRequired_RendersDefaultOnlyOnOptional()
    {
        string output = Render("    public void M1(string name, int count = 3) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(name: string, count: number = 3): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, name, count);
  }
}

""");
    }

    [Test]
    public void OptionalParameter_SameClassConst_RendersResolvedValue()
    {
        string output = Render("""
                public const int DefaultCount = 42;

                public void M1(int count = DefaultCount) {}
        """);

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(count: number = 42): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, count);
  }
}

""");
    }

    [Test]
    public void OptionalNullableValueType_RendersNullAndValueDefaults()
    {
        string output = Render("    public void M1(int? maybe = null, int? some = 7) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(maybe: number | null = null, some: number | null = 7): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, maybe, some);
  }
}

""");
    }

    [Test]
    public void ValueTypeDefaultLiteral_RendersZeroValued()
    {
        string output = Render("    public void M1(int count = default) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(count: number = 0): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, count);
  }
}

""");
    }

    [Test]
    public void ReferenceTypeNullDefault_ThrowsToHaltCodegen()
    {
        // Non-nullable reference-type null defaults would violate strictNullChecks; halt loudly until nullability handling.
        Assert.Throws<NotSupportedDefaultValueException>(
            () => Render("    public void M1(string label = null) {}"));
    }

    [Test]
    public void DateTimeDefaultLiteral_ThrowsToHaltCodegen()
    {
        // DateTime 'default' requires a dedicated Date literal (handled in a later step); halt loudly until then.
        Assert.Throws<NotSupportedDefaultValueException>(
            () => Render("    public void M1(DateTime when = default) {}"));
    }
}
