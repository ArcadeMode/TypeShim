using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassProxyRendererTests_ParameterAttributes
{
    private static string Render(string classBody)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText($$"""
            using System;
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;
            using System.Threading.Tasks;
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
    public void OptionalAndDefaultParameterValue_RendersAsRequired()
    {
        string output = Render("    public void M1([Optional, DefaultParameterValue(19.99)] double price) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(price: number): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, price);
  }
}

""");
    }

    [Test]
    public void OptionalAttributeAlone_RendersAsRequired()
    {
        string output = Render("    public void M1([Optional] double price) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(price: number): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, price);
  }
}

""");
    }

    [Test]
    public void DefaultParameterValueWithoutOptional_RendersAsRequired()
    {
        string output = Render("    public void M1([DefaultParameterValue(19.99)] double price) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(price: number): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, price);
  }
}

""");
    }

    [Test]
    public void CallerMemberName_RendersAsOrdinaryOptionalString()
    {
        string output = Render("    public void M1(string message, [CallerMemberName] string caller = \"\") {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(message: string, caller: string = ""): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, message, caller);
  }
}

""");
    }

    [Test]
    public void CallerMemberName_NullableNullDefault_RendersAsOptionalNull()
    {
        string output = Render("    public void M1(string message, [CallerMemberName] string? caller = null) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(message: string, caller: string | null = null): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, message, caller);
  }
}

""");
    }

    [Test]
    public void CallerLineNumber_RendersAsOrdinaryOptionalInt()
    {
        string output = Render("    public void M1(string message, [CallerLineNumber] int line = 0) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(message: string, line: number = 0): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, message, line);
  }
}

""");
    }

    [Test]
    public void CallerArgumentExpression_RendersAsOrdinaryOptionalString()
    {
        string output = Render("    public void M1(string value, [CallerArgumentExpression(nameof(value))] string? expr = null) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(value: string, expr: string | null = null): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, value, expr);
  }
}

""");
    }

    [Test]
    public void OptionalAndDefaultParameterValue_OnConstructor_RendersAsRequired()
    {
        string output = Render("    public C1([Optional, DefaultParameterValue(19.99)] double price) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor(price: number) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor(price));
  }

}

""");
    }

    [Test]
    public void CallerMemberName_OnConstructor_RendersAsOrdinaryOptionalString()
    {
        string output = Render("    public C1([CallerMemberName] string caller = \"\") {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor(caller: string = "") {
    super(TypeShimConfig.exports.N1.C1Interop.ctor(caller));
  }

}

""");
    }
}
