using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptJSDocTests
{
    [Test]
    public void TypeScriptUserClassProxy_ClassWithSummaryComment_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            /// <summary>
            /// This is a sample class for testing.
            /// </summary>
            [TSExport]
            public class C1
            {
                public void DoSomething() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
/**
 * This is a sample class for testing.
 */
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public DoSomething(): void {
    TypeShimConfig.exports.N1.C1Interop.DoSomething(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithSummaryComment_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// This method does something important.
                /// </summary>
                public void DoSomething() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * This method does something important.
   */
  public DoSomething(): void {
    TypeShimConfig.exports.N1.C1Interop.DoSomething(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithParamAndReturn_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Calculates the sum of two numbers.
                /// </summary>
                /// <param name="a">The first number</param>
                /// <param name="b">The second number</param>
                /// <returns>The sum of a and b</returns>
                public int Add(int a, int b) { return a + b; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Calculates the sum of two numbers.
   *
   * @param a The first number
   * @param b The second number
   * @returns The sum of a and b
   */
  public Add(a: number, b: number): number {
    return TypeShimConfig.exports.N1.C1Interop.Add(this.instance, a, b);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithException_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Divides two numbers.
                /// </summary>
                /// <param name="a">The dividend</param>
                /// <param name="b">The divisor</param>
                /// <returns>The quotient</returns>
                /// <exception cref="System.DivideByZeroException">Thrown when b is zero</exception>
                public double Divide(double a, double b) { return a / b; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Divides two numbers.
   *
   * @param a The dividend
   * @param b The divisor
   * @returns The quotient
   * @throws {System.DivideByZeroException} Thrown when b is zero
   */
  public Divide(a: number, b: number): number {
    return TypeShimConfig.exports.N1.C1Interop.Divide(this.instance, a, b);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_PropertyWithComment_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Gets or sets the name.
                /// </summary>
                public string Name { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(jsObject: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject }));
  }

  /**
   * Gets or sets the name.
   */
  public get Name(): string {
    return TypeShimConfig.exports.N1.C1Interop.get_Name(this.instance);
  }

  public set Name(value: string) {
    TypeShimConfig.exports.N1.C1Interop.set_Name(this.instance, value);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithRemarks_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// This is the summary.
                /// </summary>
                /// <remarks>
                /// This is a remark with additional information.
                /// </remarks>
                public void DoSomething() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * This is the summary.
   *
   * This is a remark with additional information.
   */
  public DoSomething(): void {
    TypeShimConfig.exports.N1.C1Interop.DoSomething(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithCodeTag_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Use this method like: <c>DoSomething()</c> to execute it.
                /// </summary>
                public void DoSomething() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Use this method like: `DoSomething()` to execute it.
   */
  public DoSomething(): void {
    TypeShimConfig.exports.N1.C1Interop.DoSomething(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithParamRef_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// This method uses <paramref name="value"/> to do something.
                /// </summary>
                /// <param name="value">The input value</param>
                public void Process(int value) {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * This method uses `value` to do something.
   *
   * @param value The input value
   */
  public Process(value: number): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance, value);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithReturnsOnly_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <returns>A random number</returns>
                public int GetRandom() { return 42; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * @returns A random number
   */
  public GetRandom(): number {
    return TypeShimConfig.exports.N1.C1Interop.GetRandom(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithParamsOnly_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <param name="name">The name to print</param>
                public void PrintName(string name) {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * @param name The name to print
   */
  public PrintName(name: string): void {
    TypeShimConfig.exports.N1.C1Interop.PrintName(this.instance, name);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithoutComment_NoJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                public void DoSomething() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public DoSomething(): void {
    TypeShimConfig.exports.N1.C1Interop.DoSomething(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithBoldTag_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// This is <b>important</b> information.
                /// </summary>
                public void DoSomething() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * This is **important** information.
   */
  public DoSomething(): void {
    TypeShimConfig.exports.N1.C1Interop.DoSomething(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithItalicTag_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// This method is <i>deprecated</i> and should not be used.
                /// </summary>
                public void DoSomething() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * This method is *deprecated* and should not be used.
   */
  public DoSomething(): void {
    TypeShimConfig.exports.N1.C1Interop.DoSomething(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParamWithInnerTags_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Processes the value.
                /// </summary>
                /// <param name="value">The <b>input</b> value, must be <c>positive</c> and <i>non-zero</i></param>
                public void Process(int value) {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Processes the value.
   *
   * @param value The **input** value, must be `positive` and *non-zero*
   */
  public Process(value: number): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance, value);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ReturnsWithInnerTags_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Gets the result.
                /// </summary>
                /// <returns>A <b>computed</b> value using <c>algorithm</c> that is <i>optimized</i></returns>
                public int GetResult() { return 42; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Gets the result.
   *
   * @returns A **computed** value using `algorithm` that is *optimized*
   */
  public GetResult(): number {
    return TypeShimConfig.exports.N1.C1Interop.GetResult(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ThrowsWithInnerTags_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Divides numbers.
                /// </summary>
                /// <param name="a">The dividend</param>
                /// <param name="b">The divisor</param>
                /// <returns>The quotient</returns>
                /// <exception cref="System.DivideByZeroException">Thrown when <paramref name="b"/> is <c>zero</c> or <i>invalid</i></exception>
                public double Divide(double a, double b) { return a / b; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Divides numbers.
   *
   * @param a The dividend
   * @param b The divisor
   * @returns The quotient
   * @throws {System.DivideByZeroException} Thrown when `b` is `zero` or *invalid*
   */
  public Divide(a: number, b: number): number {
    return TypeShimConfig.exports.N1.C1Interop.Divide(this.instance, a, b);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithListTag_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Processes items in the following order:
                /// <list type="bullet">
                /// <item><term>First</term><description>Initialize the system</description></item>
                /// <item><term>Second</term><description>Process the data</description></item>
                /// <item><term>Third</term><description>Clean up resources</description></item>
                /// </list>
                /// </summary>
                public void Process() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Processes items in the following order:
   * - First: Initialize the system
   * - Second: Process the data
   * - Third: Clean up resources
   */
  public Process(): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithMultipleParamRefs_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Compares <paramref name="left"/> with <paramref name="right"/> and returns true if <paramref name="left"/> equals <paramref name="right"/>.
                /// </summary>
                /// <param name="left">The first value</param>
                /// <param name="right">The second value</param>
                /// <returns>True if equal</returns>
                public bool Compare(int left, int right) { return left == right; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Compares `left` with `right` and returns true if `left` equals `right`.
   *
   * @param left The first value
   * @param right The second value
   * @returns True if equal
   */
  public Compare(left: number, right: number): boolean {
    return TypeShimConfig.exports.N1.C1Interop.Compare(this.instance, left, right);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ExceptionWithoutNamespace_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Performs an operation.
                /// </summary>
                /// <exception cref="InvalidOperationException">Thrown when operation is invalid</exception>
                /// <exception cref="ArgumentException">Thrown when argument is invalid</exception>
                public void DoOperation() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Performs an operation.
   *
   * @throws {System.InvalidOperationException} Thrown when operation is invalid
   * @throws {System.ArgumentException} Thrown when argument is invalid
   */
  public DoOperation(): void {
    TypeShimConfig.exports.N1.C1Interop.DoOperation(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MissingClosingTagInParam_RendersNoJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Processes a value.
                /// </summary>
                /// <param name="value">This is <b>important text that never closes
                public void Process(int value) {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public Process(value: number): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance, value);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MissingClosingTagInSummary_RendersNoJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// This method has <b>unclosed bold tag.
                /// </summary>
                public void DoSomething() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public DoSomething(): void {
    TypeShimConfig.exports.N1.C1Interop.DoSomething(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MalformedParamTag_RendersNoJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Processes a value.
                /// </summary>
                /// <param name="value">This has no closing param tag
                /// <returns>The result</returns>
                public int Process(int value) { return value; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public Process(value: number): number {
    return TypeShimConfig.exports.N1.C1Interop.Process(this.instance, value);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_UnknownExceptionType_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Processes data that may fail.
                /// </summary>
                /// <exception cref="MissingImportException">Thrown when import is missing</exception>
                public void ProcessData() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Processes data that may fail.
   *
   * @throws {MissingImportException} Thrown when import is missing
   */
  public ProcessData(): void {
    TypeShimConfig.exports.N1.C1Interop.ProcessData(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ListWithOnlyListItems_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Items to process:
                /// <list type="bullet">
                /// <item>First item</item>
                /// <item>Second item</item>
                /// <item>Third item</item>
                /// </list>
                /// </summary>
                public void Process() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Items to process:
   * First item
   * Second item
   * Third item
   */
  public Process(): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ListWithOnlyTerms_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Key concepts:
                /// <list type="bullet">
                /// <item><term>First</term></item>
                /// <item><term>Second</term></item>
                /// <item><term>Third</term></item>
                /// </list>
                /// </summary>
                public void Process() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Key concepts:
   * - First
   * - Second
   * - Third
   */
  public Process(): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ListWithStraightText_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Items:
                /// <list type="bullet">
                /// First item text
                /// Second item text
                /// Third item text
                /// </list>
                /// </summary>
                public void Process() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Items:
   */
  public Process(): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ListWithListHeader_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Processing steps:
                /// <list type="table">
                /// <listheader>
                /// <term>Step</term>
                /// <description>Action</description>
                /// </listheader>
                /// <item>
                /// <term>First</term>
                /// <description>Initialize</description>
                /// </item>
                /// <item>
                /// <term>Second</term>
                /// <description>Process</description>
                /// </item>
                /// </list>
                /// </summary>
                public void Process() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Processing steps:
   * - Step: Action
   * - First: Initialize
   * - Second: Process
   */
  public Process(): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ListWithHeaderAndOnlyTerms_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Options:
                /// <list type="bullet">
                /// <listheader>
                /// <term>Available Options</term>
                /// </listheader>
                /// <item><term>Option A</term></item>
                /// <item><term>Option B</term></item>
                /// </list>
                /// </summary>
                public void Process() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Options:
   * - Available Options
   * - Option A
   * - Option B
   */
  public Process(): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ListWithHeaderAndStraightText_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Notes:
                /// <list type="number">
                /// <listheader>Header text here</listheader>
                /// First note
                /// Second note
                /// </list>
                /// </summary>
                public void Process() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Notes:
   * Header text here
   */
  public Process(): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithBrTag_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// First line of text.<br/>
                /// Second line of text.<br/>
                /// Third line of text.
                /// </summary>
                public void Process() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * First line of text.
   * Second line of text.
   * Third line of text.
   */
  public Process(): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_BrInsideBoldTag_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// This is <b>important text<br/>spanning multiple lines</b> in the description.
                /// </summary>
                public void Process() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * This is **important text
   * spanning multiple lines** in the description.
   */
  public Process(): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_BrInsideCodeTag_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Use this code: <c>var x = 1;<br/>var y = 2;</c> for initialization.
                /// </summary>
                public void Process() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Use this code: `var x = 1;
   * var y = 2;` for initialization.
   */
  public Process(): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_BrInsideListItem_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Steps to follow:
                /// <list type="bullet">
                /// <item><description>First step<br/>with details</description></item>
                /// <item><description>Second step<br/>with more info</description></item>
                /// </list>
                /// </summary>
                public void Process() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Steps to follow:
   * - First step
   * with details
   * - Second step
   * with more info
   */
  public Process(): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_UnknownTag_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// This has an <unknowntag>unknown tag</unknowntag> in the text.
                /// </summary>
                public void Process() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * This has an unknown tag in the text.
   */
  public Process(): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ExampleTag_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Use this method like: <example>Process(data)</example> to process data.
                /// </summary>
                public void Process(string data) {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Use this method like: `Process(data)` to process data.
   */
  public Process(data: string): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance, data);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ListStartsOnNewLineWithoutBrTag_RendersJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// This method performs the following steps:
                /// <list type="number">
                /// <item><description>Step one</description></item>
                /// <item><description>Step two</description></item>
                /// </list>
                /// After the steps are complete, it returns.
                /// </summary>
                public void Process() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * This method performs the following steps:
   * - Step one
   * - Step two
   * After the steps are complete, it returns.
   */
  public Process(): void {
    TypeShimConfig.exports.N1.C1Interop.Process(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParamWithEmptyName_IgnoresParam()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Method with empty param name.
                /// </summary>
                /// <param name="">This param should be ignored</param>
                /// <param name="value">This param should be included</param>
                public void DoSomething(int value) {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Method with empty param name.
   *
   * @param value This param should be included
   */
  public DoSomething(value: number): void {
    TypeShimConfig.exports.N1.C1Interop.DoSomething(this.instance, value);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ExceptionWithEmptyCref_IgnoresException()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                /// <summary>
                /// Method with empty exception cref.
                /// </summary>
                /// <exception cref="">This exception should be ignored</exception>
                /// <exception cref="System.InvalidOperationException">This exception should be included</exception>
                public void DoSomething() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  /**
   * Method with empty exception cref.
   *
   * @throws {System.InvalidOperationException} This exception should be included
   */
  public DoSomething(): void {
    TypeShimConfig.exports.N1.C1Interop.DoSomething(this.instance);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_SeeTagWithNestedClass_PreservesOuterClassName()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            
            public class OuterClass
            {
                public class InnerClass
                {
                }
            }
            
            /// <summary>
            /// This method references <see cref="OuterClass.InnerClass"/> in the documentation.
            /// </summary>
            [TSExport]
            public class C1
            {
                public void DoSomething() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
/**
 * This method references `N1.OuterClass.InnerClass` in the documentation.
 */
export class C1 extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.N1.C1Interop.ctor());
  }

  public DoSomething(): void {
    TypeShimConfig.exports.N1.C1Interop.DoSomething(this.instance);
  }
}

""");
    }
}
