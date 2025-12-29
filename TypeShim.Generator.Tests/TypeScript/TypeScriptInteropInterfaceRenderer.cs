using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypescriptAssemblyExportsRendererTests
{
    [Test]
    public void TypescriptAssemblyExportsRenderer_InstanceMethod_WithNullableUserClassParameterType_HasObjectOrNullType()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1.N2;
            [TSExport]
            public class UserClass
            {
                public int Id { get; set; }
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            using N1.N2;
            namespace N1;
            [TSExport]
            public static class C1
            {
                public static void DoStuff(UserClass? u) {}
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
        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo, userClassInfo], symbolNameProvider);

        RenderContext renderCtx = new(null, [classInfo, userClassInfo], RenderOptions.TypeScript);
        string interopClass = new TypescriptAssemblyExportsRenderer(hierarchyInfo, symbolNameProvider, renderCtx).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      DoStuff(u: object | null): void;
    };
    N2: {
      UserClassInterop: {
        get_Id(instance: object): number;
        set_Id(instance: object, value: number): void;
      };
    };
  };
}

"""));
    }

    [TestCase("bool", "boolean")]
    [TestCase("int", "number")]
    [TestCase("string", "string")]
    public void TypeScriptInteropInterfaceRenderer_InstanceMethod_WithPrimitiveParameterType_RendersJSType(string csType, string tsType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public void DoStuff({{csType}} u) {}
            }
        """.Replace("{{csType}}", csType));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);
        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo], symbolNameProvider);

        RenderContext renderCtx = new(null, [classInfo], RenderOptions.TypeScript);
        string interopClass = new TypescriptAssemblyExportsRenderer(hierarchyInfo, symbolNameProvider, renderCtx).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      DoStuff(instance: object, u: {{tsType}}): void;
    };
  };
}

""".Replace("{{tsType}}", tsType)));
    }

    [Test]
    public void TypeScriptInteropInterfaceRenderer_InstanceMethod_WithNullableUserClassTaskParameterType_HasObjectOrNullType()
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
            public class C1
            {
                public void DoStuff(Task<UserClass?> u) {}
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

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);
        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo], symbolNameProvider);

        RenderContext renderCtx = new(null, [classInfo], RenderOptions.TypeScript);
        string interopClass = new TypescriptAssemblyExportsRenderer(hierarchyInfo, symbolNameProvider, renderCtx).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      DoStuff(instance: object, u: Promise<object | null>): void;
    };
  };
}

"""));
    }

    [Test]
    public void TypeScriptInteropInterfaceRenderer_InstanceMethod_WithNullableUserClassArrayParameterType_HasObjectOrNullType()
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
            public class C1
            {
                public void DoStuff(UserClass?[] u) {}
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

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);
        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo], symbolNameProvider);

        RenderContext renderCtx = new(null, [classInfo], RenderOptions.TypeScript);
        string interopClass = new TypescriptAssemblyExportsRenderer(hierarchyInfo, symbolNameProvider, renderCtx).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      DoStuff(instance: object, u: Array<object | null>): void;
    };
  };
}

"""));
    }

    [Test]
    public void TypeScriptInteropInterfaceRenderer_InstanceMethod_WithUserClassNullableArrayParameterType_HasObjectOrNullType()
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
            public class C1
            {
                public void DoStuff(UserClass[]? u) {}
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
        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo], symbolNameProvider);

        RenderContext renderCtx = new(null, [classInfo], RenderOptions.TypeScript);
        string interopClass = new TypescriptAssemblyExportsRenderer(hierarchyInfo, symbolNameProvider, renderCtx).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      DoStuff(instance: object, u: Array<object> | null): void;
    };
  };
}

"""));
    }
}
