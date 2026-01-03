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
    public void TypescriptAssemblyExportsRenderer_StaticMethod_WithUserClassParameterType_HasManagedObjectOrObjectType()
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
                public static void DoStuff(UserClass u) {}
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

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo, userClassInfo]);
        RenderContext renderCtx = new(null, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      DoStuff(u: ManagedObject | object): void;
    };
    N2: {
      UserClassInterop: {
        ctor(jsObject: object): ManagedObject;
        get_Id(instance: ManagedObject): number;
        set_Id(instance: ManagedObject, value: number): void;
      };
    };
  };
}

""");
    }

    [Test]
    public void TypescriptAssemblyExportsRenderer_StaticMethod_WithUserClassParameterType_WithoutPublicSet_HasOnlyManagedObjectType()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1.N2;
            [TSExport]
            public class UserClass
            {
                public int Id { get; private set; } // class cannot be constructed with an initializer, so no 'object' type should be accepted in TS.
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
                public static void DoStuff(UserClass u) {}
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

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo, userClassInfo]);
        RenderContext renderCtx = new(null, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      DoStuff(u: ManagedObject): void;
    };
    N2: {
      UserClassInterop: {
        ctor(): ManagedObject;
        get_Id(instance: ManagedObject): number;
      };
    };
  };
}

""");
    }

    [Test]
    public void TypescriptAssemblyExportsRenderer_StaticMethod_WithNullableUserClassParameterType_HasManagedObjectOrObjectOrNullType()
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

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo, userClassInfo]);
        RenderContext renderCtx = new(null, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      DoStuff(u: ManagedObject | object | null): void;
    };
    N2: {
      UserClassInterop: {
        ctor(jsObject: object): ManagedObject;
        get_Id(instance: ManagedObject): number;
        set_Id(instance: ManagedObject, value: number): void;
      };
    };
  };
}

""");
    }

    [Test]
    public void TypescriptAssemblyExportsRenderer_StaticMethod_WithNullableUserClassParameterType_WithoutParameterlessConstructor_HasManagedObjectOrNullType()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1.N2;
            [TSExport]
            public class UserClass
            {
                public UserClass(int nowYouCannotInitializeFromJS) { }
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

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo, userClassInfo]);
        RenderContext renderCtx = new(null, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      DoStuff(u: ManagedObject | null): void;
    };
    N2: {
      UserClassInterop: {
        ctor(nowYouCannotInitializeFromJS: number, jsObject: object): ManagedObject;
        get_Id(instance: ManagedObject): number;
        set_Id(instance: ManagedObject, value: number): void;
      };
    };
  };
}

""");
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

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo]);
        RenderContext renderCtx = new(null, [classInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      ctor(): ManagedObject;
      DoStuff(instance: ManagedObject, u: {{tsType}}): void;
    };
  };
}

""".Replace("{{tsType}}", tsType));
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

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo, userClassInfo]);
        RenderContext renderCtx = new(null, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      ctor(): ManagedObject;
      DoStuff(instance: ManagedObject, u: Promise<ManagedObject | object | null>): void;
    };
    UserClassInterop: {
      ctor(jsObject: object): ManagedObject;
      get_Id(instance: ManagedObject): number;
      set_Id(instance: ManagedObject, value: number): void;
    };
  };
}

""");
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

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo, userClassInfo]);
        RenderContext renderCtx = new(null, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """    
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      ctor(): ManagedObject;
      DoStuff(instance: ManagedObject, u: Array<ManagedObject | object | null>): void;
    };
    UserClassInterop: {
      ctor(jsObject: object): ManagedObject;
      get_Id(instance: ManagedObject): number;
      set_Id(instance: ManagedObject, value: number): void;
    };
  };
}

""");
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

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo]); //deliberate omit userClassInfo to reduce noise in baseline
        RenderContext renderCtx = new(null, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      ctor(): ManagedObject;
      DoStuff(instance: ManagedObject, u: Array<ManagedObject | object> | null): void;
    };
  };
}

""");
    }

    [Test]
    public void TypeScriptInteropInterfaceRenderer_InstanceMethod_WithObjectParameterType_HasManagedObjectType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public void DoStuff(object u) {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo]);
        RenderContext renderCtx = new(null, [classInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """    
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      ctor(): ManagedObject;
      DoStuff(instance: ManagedObject, u: ManagedObject): void;
    };
  };
}

""");
    }

    [Test]
    public void TypeScriptInteropInterfaceRenderer_ParameterlessConstructorWithPrimitiveProperty_RendersInitializerObjectInCtor()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public string P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo]);
        RenderContext renderCtx = new(null, [classInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      ctor(jsObject: object): ManagedObject;
      get_P1(instance: ManagedObject): string;
      set_P1(instance: ManagedObject, value: string): void;
    };
  };
}

""");
    }

    [Test]
    public void TypeScriptInteropInterfaceRenderer_ParameterizedConstructorWithPrimitiveProperty_RendersParametersAndInitializerObjectInCtor()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1(int i)
            {
                public string P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo]);
        RenderContext renderCtx = new(null, [classInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      ctor(i: number, jsObject: object): ManagedObject;
      get_P1(instance: ManagedObject): string;
      set_P1(instance: ManagedObject, value: string): void;
    };
  };
}

""");
    }

    [Test]
    public void TypeScriptInteropInterfaceRenderer_ParameterizedConstructorWithoutSetProperties_RendersNoInitializerObjectInCtor()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1(int i, string s, object o)
            {
                public string P1 { get; internal init; }
                public int P2 { get; internal set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo]);
        RenderContext renderCtx = new(null, [classInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1Interop: {
      ctor(i: number, s: string, o: ManagedObject): ManagedObject;
      get_P1(instance: ManagedObject): string;
      get_P2(instance: ManagedObject): number;
    };
  };
}

""");
    }
}
