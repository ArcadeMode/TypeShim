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
    public void DateTimeDefaultLiteral_RendersMinValueDate()
    {
        // DateTime 'default' is DateTime.MinValue; the literal round-trips to MinValue across the marshaller.
        string output = Render("    public void M1(DateTime when = default) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(when: Date = new Date("0001-01-01T00:00:00Z")): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, when);
  }
}

""");
    }

    [Test]
    public void DateTimeOffsetDefaultLiteral_RendersMinValueDate()
    {
        string output = Render("    public void M1(DateTimeOffset when = default) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(when: Date = new Date("0001-01-01T00:00:00Z")): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, when);
  }
}

""");
    }

    [Test]
    public void CharDefault_RendersQuotedString()
    {
        string output = Render("    public void M1(char c = 'A') {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(c: string = "A"): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, c.charCodeAt(0));
  }
}

""");
    }

    [Test]
    public void NullableCharDefaultNull_RendersNull()
    {
        string output = Render("    public void M1(char? c = null) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(c: string | null = null): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, c ? c.charCodeAt(0) : null);
  }
}

""");
    }

    [Test]
    public void NullableIntArrayDefaultNull_RendersNull()
    {
        string output = Render("    public void M1(int[]? values = null) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(values: Array<number> | null = null): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, values);
  }
}

""");
    }

    [Test]
    public void NullableStringArrayDefaultLiteral_RendersNull()
    {
        string output = Render("    public void M1(string[]? values = default) {}");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public M1(values: Array<string> | null = null): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, values);
  }
}

""");
    }

    [Test]
    public void NonNullableArrayDefault_Throws()
    {
        // Non-nullable reference type with a null default; rejected pending nullable widening.
        Assert.Throws<NotSupportedDefaultValueException>(() => Render("    public void M1(int[] values = default) {}"));
    }

    [Test]
    public void NullableTaskDefaultNull_RendersNull()
    {
        string output = Render("    public Task M1(Task<int>? work = null) => Task.CompletedTask;");

        AssertEx.EqualOrDiff(output, """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public async M1(work: Promise<number> | null = null): Promise<void> {
    return TypeShimConfig.exports.N1.C1Interop.M1(this.instance, work);
  }
}

""");
    }
}
