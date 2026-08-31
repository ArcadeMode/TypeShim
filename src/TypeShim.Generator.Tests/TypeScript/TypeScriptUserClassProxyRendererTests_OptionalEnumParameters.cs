using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassProxyRendererTests_OptionalEnumParameters
{
    private static string Render(string classBody)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText($$"""
            using System;
            namespace N1;
            [TSExport]
            public enum Priority { Low, Medium, High }
            [TSExport]
            public class C1
            {
            {{classBody}}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedSymbols = [.. symbolExtractor.ExtractAllExportedSymbols()];

        InteropTypeInfoCache typeCache = new();
        List<NamedTypeInfo> allNamedTypes = [.. exportedSymbols
            .Select(s => new NamedTypeInfoBuilder(s, typeCache).Build())
            .OfType<NamedTypeInfo>()];
        ClassInfo classInfo = allNamedTypes.OfType<ClassInfo>().Single(c => c.Name == "C1");

        RenderContext renderContext = new(classInfo, allNamedTypes, RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();
        return renderContext.ToString();
    }

    [Test]
    public void EnumDefault_RendersNamedMember()
    {
        string output = Render("    public void M1(Priority p = Priority.Medium) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(p: Priority = Priority.Medium): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, p);
  }
}

""");
    }

    [Test]
    public void EnumDefaultLiteral_RendersZeroMember()
    {
        string output = Render("    public void M1(Priority p = default) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(p: Priority = Priority.Low): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, p);
  }
}

""");
    }

    [Test]
    public void EnumCastLiteral_MappingToMember_RendersNamedMember()
    {
        // A cast default like '(Priority)1' isn't idiomatic, but must still produce valid TypeScript;
        // when the value maps to a member it resolves to the named member.
        string output = Render("    public void M1(Priority p = (Priority)1) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(p: Priority = Priority.Medium): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, p);
  }
}

""");
    }

    [Test]
    public void EnumValueWithoutMember_RendersNumericFallback()
    {
        // No member has value 99, so codegen falls back to the numeric literal (still valid TS).
        string output = Render("    public void M1(Priority p = (Priority)99) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(p: Priority = 99): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, p);
  }
}

""");
    }

    [Test]
    public void NullableEnumDefaultNull_RendersNull()
    {
        string output = Render("    public void M1(Priority? p = null) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(p: Priority | null = null): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, p);
  }
}

""");
    }

    [Test]
    public void NullableEnumWithValue_RendersNamedMember()
    {
        string output = Render("    public void M1(Priority? p = Priority.High) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(p: Priority | null = Priority.High): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, p);
  }
}

""");
    }

    [Test]
    public void NullableEnumArrayDefaultNull_RendersNull()
    {
        string output = Render("    public void M1(Priority[]? p = null) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(p: Array<Priority> | null = null): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, p);
  }
}

""");
    }
}
