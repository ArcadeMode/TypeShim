using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptRendererTests_Properties
{
    [TestCase("string", "string")]
    [TestCase("double", "number")]
    [TestCase("bool", "boolean")]
    public void TypeScriptUserClassProxy_InstanceProperty_GeneratesGetAndSetFunctions(string typeExpression, string typeScriptType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public {{typeExpression}} P1 { get; set; }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);
        TypeScriptMethodRenderer methodRenderer = new(symbolNameProvider);


        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public get P1(): {{typeScriptType}} {
    return this.interop.N1.C1Interop.get_P1(this.instance);
  }

  public set P1(value: {{typeScriptType}}) {
    this.interop.N1.C1Interop.set_P1(this.instance, value);
  }

}

""".Replace("{{typeScriptType}}", typeScriptType)));
    }

    [TestCase("string", "string")]
    [TestCase("double", "number")]
    [TestCase("bool", "boolean")]
    public void TypeScriptUserClassProxy_StaticProperty_GeneratesNothing(string typeExpression, string typeScriptType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public static {{typeExpression}} P1 { get; set; }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);
        TypeScriptMethodRenderer methodRenderer = new(symbolNameProvider);


        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

}

""".Replace("{{typeScriptType}}", typeScriptType)));
    }

    [TestCase("string", "string")]
    [TestCase("double", "number")]
    [TestCase("bool", "boolean")]
    public void TypeScriptUserModuleClass_StaticProperty_GeneratesGetAndSetFunctions(string typeExpression, string typeScriptType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSModule]
            public static class C1
            {
                public static {{typeExpression}} P1 { get; set; }
                public static {{typeExpression}} P2 { get; init; }
                public static {{typeExpression}} P3 { get; }
                public static {{typeExpression}} P4 => 1
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);
        TypeScriptMethodRenderer methodRenderer = new(symbolNameProvider);

        ModuleInfo moduleInfo = new()
        {
            ExportedClasses = [classInfo],
            HierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo], symbolNameProvider),
        };

        string interopClass = new TypescriptUserModuleClassRenderer(classInfo, methodRenderer, symbolNameProvider).Render();

        Assert.That(interopClass, Is.EqualTo("""
// Auto-generated TypeShim TSModule class. Source class: N1.C1
export class C1 {
  private interop: AssemblyExports;

  constructor(interop: AssemblyExports) {
    this.interop = interop;
  }

  public get P1(): {{typeScriptType}} {
    return this.interop.N1.C1Interop.get_P1();
  }

  public set P1(value: {{typeScriptType}}) {
    this.interop.N1.C1Interop.set_P1(value);
  }

  public get P2(): {{typeScriptType}} {
    return this.interop.N1.C1Interop.get_P2();
  }

  public set P2(value: {{typeScriptType}}) {
    this.interop.N1.C1Interop.set_P2(value);
  }

  public get P3(): {{typeScriptType}} {
    return this.interop.N1.C1Interop.get_P3();
  }

  public get P4(): {{typeScriptType}} {
    return this.interop.N1.C1Interop.get_P4();
  }

}

""".Replace("{{typeScriptType}}", typeScriptType)));
    }
}
