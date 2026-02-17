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
    public void TypeScriptUserClassProxy_ComprehensiveClassWithVariousComments_RendersAllJSDoc()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            
            /// <summary>
            /// A comprehensive calculator class demonstrating various comment types.
            /// </summary>
            /// <remarks>
            /// This class showcases how different XML documentation elements are transformed
            /// into JSDoc format for TypeScript consumption.
            /// </remarks>
            [TSExport]
            public class Calculator
            {
                /// <summary>
                /// Gets or sets the current result value.
                /// </summary>
                /// <remarks>
                /// This property stores the most recent calculation result.
                /// </remarks>
                public double Result { get; set; }

                /// <summary>
                /// Gets the calculation history count.
                /// </summary>
                public int HistoryCount { get; }

                /// <summary>
                /// Adds two numbers and returns the sum.
                /// </summary>
                /// <param name="a">The first number to add</param>
                /// <param name="b">The second number to add</param>
                /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/></returns>
                public double Add(double a, double b) { return a + b; }

                /// <summary>
                /// Subtracts <paramref name="b"/> from <paramref name="a"/>.
                /// </summary>
                /// <param name="a">The minuend</param>
                /// <param name="b">The subtrahend</param>
                /// <returns>The difference between the two numbers</returns>
                public double Subtract(double a, double b) { return a - b; }

                /// <summary>
                /// Divides one number by another with error handling.
                /// </summary>
                /// <param name="numerator">The number to be divided</param>
                /// <param name="denominator">The number to divide by</param>
                /// <returns>The quotient of the division</returns>
                /// <exception cref="System.DivideByZeroException">Thrown when <paramref name="denominator"/> is zero</exception>
                /// <exception cref="System.ArgumentException">Thrown when inputs are invalid</exception>
                public double Divide(double numerator, double denominator) { return numerator / denominator; }

                /// <summary>
                /// Calculates the power of a number using the <c>Math.Pow</c> method.
                /// </summary>
                /// <param name="baseNumber">The base number</param>
                /// <param name="exponent">The exponent to raise to</param>
                /// <returns>The result of <paramref name="baseNumber"/> raised to <paramref name="exponent"/></returns>
                public double Power(double baseNumber, double exponent) { return Math.Pow(baseNumber, exponent); }

                /// <summary>
                /// Clears all calculation history and resets the calculator.
                /// </summary>
                /// <remarks>
                /// This operation cannot be undone. Use with caution.
                /// </remarks>
                public void Clear() { }
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
 * A comprehensive calculator class demonstrating various comment types.
 *
 * This class showcases how different XML documentation elements are transformed into JSDoc format for TypeScript consumption.
 */
export class Calculator extends ProxyBase {
  constructor(jsObject: Calculator.Initializer) {
    super(TypeShimConfig.exports.N1.CalculatorInterop.ctor({ ...jsObject }));
  }

  /**
   * Adds two numbers and returns the sum.
   *
   * @param a The first number to add
   * @param b The second number to add
   * @returns The sum of `a` and `b`
   */
  public Add(a: number, b: number): number {
    return TypeShimConfig.exports.N1.CalculatorInterop.Add(this.instance, a, b);
  }

  /**
   * Subtracts `b` from `a`.
   *
   * @param a The minuend
   * @param b The subtrahend
   * @returns The difference between the two numbers
   */
  public Subtract(a: number, b: number): number {
    return TypeShimConfig.exports.N1.CalculatorInterop.Subtract(this.instance, a, b);
  }

  /**
   * Divides one number by another with error handling.
   *
   * @param numerator The number to be divided
   * @param denominator The number to divide by
   * @returns The quotient of the division
   * @throws {System.DivideByZeroException} Thrown when `denominator` is zero
   * @throws {System.ArgumentException} Thrown when inputs are invalid
   */
  public Divide(numerator: number, denominator: number): number {
    return TypeShimConfig.exports.N1.CalculatorInterop.Divide(this.instance, numerator, denominator);
  }

  /**
   * Calculates the power of a number using the `Math.Pow` method.
   *
   * @param baseNumber The base number
   * @param exponent The exponent to raise to
   * @returns The result of `baseNumber` raised to `exponent`
   */
  public Power(baseNumber: number, exponent: number): number {
    return TypeShimConfig.exports.N1.CalculatorInterop.Power(this.instance, baseNumber, exponent);
  }

  /**
   * Clears all calculation history and resets the calculator.
   *
   * This operation cannot be undone. Use with caution.
   */
  public Clear(): void {
    TypeShimConfig.exports.N1.CalculatorInterop.Clear(this.instance);
  }
  /**
   * Gets or sets the current result value.
   *
   * This property stores the most recent calculation result.
   */
  public get Result(): number {
    return TypeShimConfig.exports.N1.CalculatorInterop.get_Result(this.instance);
  }

  public set Result(value: number) {
    TypeShimConfig.exports.N1.CalculatorInterop.set_Result(this.instance, value);
  }

  /**
   * Gets the calculation history count.
   */
  public get HistoryCount(): number {
    return TypeShimConfig.exports.N1.CalculatorInterop.get_HistoryCount(this.instance);
  }
}

""");
    }
}
