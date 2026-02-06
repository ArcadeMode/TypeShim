using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassProxyRendererTests_Char
{
    [Test]
    public void TypeScriptUserClassProxy_InstanceMethod_WithCharReturnType_RendersNumberToStringConversion()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public char M1() {}
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

  public M1(): string {
    const res = TypeShimConfig.exports.N1.C1Interop.M1(this.instance);
    return String.fromCharCode(res);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceMethod_WithCharParameterType_RendersNumberToStringConversion()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public void M1(char p1) {}
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

  public M1(p1: string): void {
    TypeShimConfig.exports.N1.C1Interop.M1(this.instance, p1.charCodeAt(0));
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceMethod_WithCharParameterAndReturnType_RendersNumberToStringConversion()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public char M1(char p1) => p1;
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

  public M1(p1: string): string {
    const res = TypeShimConfig.exports.N1.C1Interop.M1(this.instance, p1.charCodeAt(0));
    return String.fromCharCode(res);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithCharType_RendersNumberToStringConversion()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public char P1 { get; set; }
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
            super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: jsObject.P1.charCodeAt(0) }));
          }

          public get P1(): string {
            const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
            return String.fromCharCode(res);
          }

          public set P1(value: string) {
            TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value.charCodeAt(0));
          }
        }

        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithNullableCharType_RendersNumberToStringConversion()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public char? P1 { get; set; }
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
            super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: jsObject.P1 ? jsObject.P1.charCodeAt(0) : null }));
          }

          public get P1(): string | null {
            const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
            return res ? String.fromCharCode(res) : null;
          }

          public set P1(value: string | null) {
            TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value ? value.charCodeAt(0) : null);
          }
        }

        """);
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithCharTaskType_RendersNumberToStringConversion()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public Task<char> P1 { get; set; }
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
            super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: jsObject.P1.then(e => e.charCodeAt(0)) }));
          }

          public get P1(): Promise<string> {
            const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
            return res.then(e => String.fromCharCode(e));
          }

          public set P1(value: Promise<string>) {
            TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value.then(e => e.charCodeAt(0)));
          }
        }
        
        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithCharNullableTaskType_RendersNumberToStringConversion()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public Task<char>? P1 { get; set; }
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
            super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: jsObject.P1 ? jsObject.P1.then(e => e.charCodeAt(0)) : null }));
          }

          public get P1(): Promise<string> | null {
            const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
            return res ? res.then(e => String.fromCharCode(e)) : null;
          }

          public set P1(value: Promise<string> | null) {
            TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value ? value.then(e => e.charCodeAt(0)) : null);
          }
        }
        
        """);
    }
}
