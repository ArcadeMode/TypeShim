using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptRendererTests_Constructors
{
    [TestCase("string", "string")]
    [TestCase("double", "number")]
    [TestCase("bool", "boolean")]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithPrimitiveParameterType(string typeExpression, string typeScriptType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1({{typeExpression}} p1)
            {
                public {{typeExpression}} P1 => default;
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();

        Assert.That(renderContext.ToString(), Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor(p1: {{typeScriptType}}) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1));
  }

  public get P1(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }
}

""".Replace("{{typeScriptType}}", typeScriptType)));
    }

    [TestCase("string?", "string | null")]
    [TestCase("double?", "number | null")]
    [TestCase("bool?", "boolean | null")]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithNullablePrimitiveParameterType(string typeExpression, string typeScriptType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1({{typeExpression}} p1)
            {
                public {{typeExpression}} P1 => default;
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();

        Assert.That(renderContext.ToString(), Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor(p1: {{typeScriptType}}) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1));
  }

  public get P1(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }
}

""".Replace("{{typeScriptType}}", typeScriptType)));
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithUserClassParameterType()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class UserClass
            {
                public int Id { get; set; }
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1(UserClass p1)
            {
                public int P1 => default;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();

        Assert.That(renderContext.ToString(), Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor(p1: UserClass.Proxy | UserClass.Snapshot) {
    const p1Instance = p1 instanceof UserClass.Proxy ? p1.instance : p1;
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1Instance));
  }

  public get P1(): number {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }
}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithNullableUserClassParameterType()
    {
        Assert.Fail("Not implemented");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithUserClassArrayParameterType()
    {
        Assert.Fail("Not implemented");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithNullableUserClassArrayParameterType()
    {
        Assert.Fail("Not implemented");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithUserClassNullableArrayParameterType()
    {
        Assert.Fail("Not implemented");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithNullableUserClassNullableArrayParameterType()
    {
        Assert.Fail("Not implemented");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithUserClassTaskParameterType()
    {
        Assert.Fail("Not implemented");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithNullableUserClassTaskParameterType()
    {
        Assert.Fail("Not implemented");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithUserClassNullableTaskParameterType()
    {
        Assert.Fail("Not implemented");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithNullableUserClassNullableTaskParameterType()
    {
        Assert.Fail("Not implemented");
    }
}
