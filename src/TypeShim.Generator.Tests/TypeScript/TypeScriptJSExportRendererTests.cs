using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptJSExportRendererTests
{
    [Test]
    public void JSExport_StaticMethod_VoidReturn_NoParams()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static void M1() {}
            }
        """);

        ClassInfo classInfo = BuildClassInfo(syntaxTree);
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 {
  private constructor() {}

  public static M1(): void {
    TypeShimConfig.exports.N1.C1.M1();
  }
}

""");
    }

    [TestCase("int", "number")]
    [TestCase("bool", "boolean")]
    [TestCase("string", "string")]
    [TestCase("double", "number")]
    public void JSExport_StaticMethod_PrimitiveReturnType(string typeExpression, string typeScriptType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static {{typeExpression}} M1() => default;
            }
        """.Replace("{{typeExpression}}", typeExpression));

        ClassInfo classInfo = BuildClassInfo(syntaxTree);
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 {
  private constructor() {}

  public static M1(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1.M1();
  }
}

""".Replace("{{typeScriptType}}", typeScriptType));
    }

    [TestCase("int", "number")]
    [TestCase("bool", "boolean")]
    [TestCase("string", "string")]
    [TestCase("double", "number")]
    public void JSExport_StaticMethod_PrimitiveParameter(string typeExpression, string typeScriptType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static void M1({{typeExpression}} p1) {}
            }
        """.Replace("{{typeExpression}}", typeExpression));

        ClassInfo classInfo = BuildClassInfo(syntaxTree);
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 {
  private constructor() {}

  public static M1(p1: {{typeScriptType}}): void {
    TypeShimConfig.exports.N1.C1.M1(p1);
  }
}

""".Replace("{{typeScriptType}}", typeScriptType));
    }

    [Test]
    public void JSExport_StaticMethod_IntArrayReturnType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static int[] M1() => Array.Empty<int>();
            }
        """);

        ClassInfo classInfo = BuildClassInfo(syntaxTree);
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 {
  private constructor() {}

  public static M1(): Array<number> {
    return TypeShimConfig.exports.N1.C1.M1();
  }
}

""");
    }

    [Test]
    public void JSExport_StaticMethod_IntArrayParameter()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static void M1(int[] p1) {}
            }
        """);

        ClassInfo classInfo = BuildClassInfo(syntaxTree);
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 {
  private constructor() {}

  public static M1(p1: Array<number>): void {
    TypeShimConfig.exports.N1.C1.M1(p1);
  }
}

""");
    }

    [Test]
    public void JSExport_StaticMethod_TaskBoolReturnType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static Task<bool> M1() => Task.FromResult(true);
            }
        """);

        ClassInfo classInfo = BuildClassInfo(syntaxTree);
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 {
  private constructor() {}

  public static async M1(): Promise<boolean> {
    return TypeShimConfig.exports.N1.C1.M1();
  }
}

""");
    }

    [Test]
    public void JSExport_StaticMethod_MultipleParameters()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static void M1(int x, bool y, string z) {}
            }
        """);

        ClassInfo classInfo = BuildClassInfo(syntaxTree);
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 {
  private constructor() {}

  public static M1(x: number, y: boolean, z: string): void {
    TypeShimConfig.exports.N1.C1.M1(x, y, z);
  }
}

""");
    }

    [Test]
    public void JSExport_AssemblyExports_UsesOriginalClassName_NoInteropSuffix()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static int M1(int p1) => p1;
            }
        """);

        ClassInfo classInfo = BuildClassInfo(syntaxTree);
        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo]);
        RenderContext renderCtx = new(null, [classInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    C1: {
      M1(p1: number): number;
    };
  };
}

""");
    }

    [Test]
    public void JSExport_TSClass_HasPrivateConstructor()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static void M1() {}
            }
        """);

        ClassInfo classInfo = BuildClassInfo(syntaxTree);
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        Assert.That(renderContext.ToString(), Does.Contain("private constructor() {}"));
        Assert.That(renderContext.ToString(), Does.Not.Contain("extends ProxyBase"));
    }

    [Test]
    public void JSExport_TwoMethodsInSameClass_BothRendered()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static int A() => 1;
                [JSExport]
                public static bool B() => true;
            }
        """);

        ClassInfo classInfo = BuildClassInfo(syntaxTree);
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 {
  private constructor() {}

  public static A(): number {
    return TypeShimConfig.exports.N1.C1.A();
  }

  public static B(): boolean {
    return TypeShimConfig.exports.N1.C1.B();
  }
}

""");
    }

    [Test]
    public void JSExport_MethodAndNonJSExportMethod_OnlyJSExportRendered()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static int Kept() => 1;
                public static int Dropped() => 2;
            }
        """);

        ClassInfo classInfo = BuildClassInfo(syntaxTree);
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        Assert.That(renderContext.ToString(), Does.Contain("public static Kept()"));
        Assert.That(renderContext.ToString(), Does.Not.Contain("Dropped"));
    }

    [Test]
    public void JSExport_TwoJSExportClasses_InSameNamespace_BothExported()
    {
        SyntaxTree treeA = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class A
            {
                [JSExport]
                public static int MA() => 1;
            }
        """);
        SyntaxTree treeB = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class B
            {
                [JSExport]
                public static int MB() => 2;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(treeA), CSharpFileInfo.Create(treeB)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo a = new ClassInfoBuilder(exportedClasses[0], typeCache).Build();
        ClassInfo b = new ClassInfoBuilder(exportedClasses[1], typeCache).Build();

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([a, b]);
        RenderContext renderCtx = new(null, [a, b], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    A: {
      MA(): number;
    };
    B: {
      MB(): number;
    };
  };
}

""");
    }

    [Test]
    public void JSExport_NestedNamespace_RendersUnderCorrectPath()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1.N2;
            public partial class C1
            {
                [JSExport]
                public static int M1() => 1;
            }
        """);

        ClassInfo classInfo = BuildClassInfo(syntaxTree);
        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo]);
        RenderContext renderCtx = new(null, [classInfo], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    N2: {
      C1: {
        M1(): number;
      };
    };
  };
}

""");
    }

    [TestCase("int?", "number | null")]
    [TestCase("string?", "string | null")]
    [TestCase("bool?", "boolean | null")]
    public void JSExport_StaticMethod_NullableReturnType(string typeExpression, string typeScriptType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static {{typeExpression}} M1() => default;
            }
        """.Replace("{{typeExpression}}", typeExpression));

        ClassInfo classInfo = BuildClassInfo(syntaxTree);
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 {
  private constructor() {}

  public static M1(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1.M1();
  }
}

""".Replace("{{typeScriptType}}", typeScriptType));
    }

    private static ClassInfo BuildClassInfo(SyntaxTree syntaxTree)
    {
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        return new ClassInfoBuilder(classSymbol, typeCache).Build();
    }
}
