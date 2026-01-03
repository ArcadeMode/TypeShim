using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassInterfaceRendererTests_Constructors
{
    [Test]
    public void UserClassInterface_PrivateConstructor_InstanceProperty_GeneratesNoInitializer()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                private C1() {}
                public string P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(symbolNameProvider, renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Snapshot {
    P1: string;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1,
    };
  }
}

""");
    }

    [Test]
    public void UserClassInterface_ParameterizedConstructor_InstanceProperty_GeneratesSnapshotAndInitializer()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public C1(int i) {}
                public string P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(symbolNameProvider, renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Initializer {
    P1: string;
  }
  export interface Snapshot {
    P1: string;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1,
    };
  }
}

""");
    }

    [Test]
    public void UserClassNamespace_ParameterlessConstructor_AndUnexportedPropertyType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public Version P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(symbolNameProvider, renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """    
export namespace C1 {
  export interface Initializer {
    P1: ManagedObject;
  }
  export interface Snapshot {
    P1: ManagedObject;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1,
    };
  }
}

""");
    }

    [Test]
    public void UserClassNamespace_ParameterizedConstructor_AndUnexportedPropertyType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1(int i)
            {
                public Version P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(symbolNameProvider, renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Initializer {
    P1: ManagedObject;
  }
  export interface Snapshot {
    P1: ManagedObject;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1,
    };
  }
}

""");
    }

    [Test]
    public void UserClassNamespace_NoExportedConstructor_AndNoExportedPropreties_GeneratesNoShapes()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class C1
        {
            private C1() {}
            private int P1 { get; set; }
            private string P2 { get; set; }
        }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(symbolNameProvider, renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """

""");
    }
}
