using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassProxyRendererTests_ParameterlessConstructors
{
    [TestCase("string", "string")]
    [TestCase("double", "number")]
    [TestCase("bool", "boolean")]
    public void TypeScriptUserClassProxy_ParameterlessConstructor_WithPrimitivePropertyType(string typeExpression, string typeScriptType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1()
            {
                public {{typeExpression}} P1 { get; set }
            }
        """.Replace("{{typeExpression}}", typeExpression));

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

  public get P1(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }

  public set P1(value: {{typeScriptType}}) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value);
  }
}

""".Replace("{{typeScriptType}}", typeScriptType));
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterlessConstructor_WithUserClassPropertyTypes()
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
            public class C1()
            {
                public UserClass P1 { get; set; }
                public UserClass P2 { get; }
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();
        // P2 is not mapped in the initializer ctor param since it has no setter.
        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(jsObject: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: jsObject.P1 instanceof UserClass ? jsObject.P1.instance : jsObject.P1 }));
  }

  public get P1(): UserClass {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return ProxyBase.fromHandle(UserClass, res);
  }

  public set P1(value: UserClass | UserClass.Initializer) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value instanceof UserClass ? value.instance : value);
  }

  public get P2(): UserClass {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P2(this.instance);
    return ProxyBase.fromHandle(UserClass, res);
  }
}

""");
    }
}
