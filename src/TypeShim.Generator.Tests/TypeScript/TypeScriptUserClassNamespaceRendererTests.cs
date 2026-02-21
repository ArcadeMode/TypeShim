using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassNamespaceRendererTests
{
    [TestCase("string", "string")]
    [TestCase("double", "number")]
    [TestCase("bool", "boolean")]
    [TestCase("string[]", "Array<string>")]
    [TestCase("double[]", "Array<number>")]
    [TestCase("ArraySegment<int>", "IMemoryView<Int32Array>")]
    [TestCase("Task<string>", "Promise<string>")]
    [TestCase("Task<double>", "Promise<number>")]
    public void UserClassNamespace_InstancePropertyOfType_GeneratesPropertyInSnapshotAndInitializer(string typeExpression, string typeScriptType)
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

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Initializer {
    P1: {{typeScriptType}};
  }
  export interface Snapshot {
    P1: {{typeScriptType}};
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1,
    };
  }
}

""".Replace("{{typeScriptType}}", typeScriptType));
    }

    [Test]
    public void UserClassNamespace_InstancePropertyOfUserClassType_GeneratesProperty()
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
                public UserClass P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userclassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userclassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Initializer {
    P1: UserClass | UserClass.Initializer;
  }
  export interface Snapshot {
    P1: UserClass.Snapshot;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: UserClass.materialize(proxy.P1),
    };
  }
}

""");
    }

    [Test]
    public void UserClassNamespace_InstancePropertyOfNotExportedClassType_GeneratesProperty()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            //[TSExport]
            public class BadUserClass
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
                public BadUserClass P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

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
    public void UserClassNamespace_InstancePropertyOfUserClassType_InitOnly_GeneratesProperty()
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
                public UserClass P1 { get; init; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userclassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userclassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Initializer {
    P1: UserClass | UserClass.Initializer;
  }
  export interface Snapshot {
    P1: UserClass.Snapshot;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: UserClass.materialize(proxy.P1),
    };
  }
}

""");
    }

    [Test]
    public void UserClassNamespace_InstancePropertyOfNullableUserClassType_GeneratesProperty()
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
                public UserClass? P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userclassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userclassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Initializer {
    P1: UserClass | UserClass.Initializer | null;
  }
  export interface Snapshot {
    P1: UserClass.Snapshot | null;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1 ? UserClass.materialize(proxy.P1) : null,
    };
  }
}

""");
    }

    [Test]
    public void UserClassNamespace_InstancePropertyOfUserClassTaskType_GeneratesProperty()
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
                public Task<UserClass> P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userclassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userclassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Initializer {
    P1: Promise<UserClass | UserClass.Initializer>;
  }
  export interface Snapshot {
    P1: Promise<UserClass.Snapshot>;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1.then(e => UserClass.materialize(e)),
    };
  }
}

""");
    }

    [Test]
    public void UserClassNamespace_InstancePropertyOfNullableUserClassTaskType_GeneratesProperty()
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
                public Task<UserClass?> P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userclassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userclassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Initializer {
    P1: Promise<UserClass | UserClass.Initializer | null>;
  }
  export interface Snapshot {
    P1: Promise<UserClass.Snapshot | null>;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1.then(e => e ? UserClass.materialize(e) : null),
    };
  }
}

""");
    }

    [Test]
    public void UserClassNamespace_InstancePropertyOfNullableUserClassNullableTaskType_GeneratesProperty()
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
                public Task<UserClass?>? P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userclassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userclassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Initializer {
    P1: Promise<UserClass | UserClass.Initializer | null> | null;
  }
  export interface Snapshot {
    P1: Promise<UserClass.Snapshot | null> | null;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1 ? proxy.P1.then(e => e ? UserClass.materialize(e) : null) : null,
    };
  }
}

""");
    }

    [Test]
    public void UserClassNamespace_InstancePropertyOfUserClassArrayType_GeneratesProperty()
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
                public UserClass[] P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userclassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userclassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Initializer {
    P1: Array<UserClass | UserClass.Initializer>;
  }
  export interface Snapshot {
    P1: Array<UserClass.Snapshot>;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1.map(e => UserClass.materialize(e)),
    };
  }
}

""");
    }

    [Test]
    public void UserClassNamespace_InstancePropertyOfNullableUserClassArrayType_GeneratesProperty()
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
                public UserClass?[] P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userclassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userclassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Initializer {
    P1: Array<UserClass | UserClass.Initializer | null>;
  }
  export interface Snapshot {
    P1: Array<UserClass.Snapshot | null>;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1.map(e => e ? UserClass.materialize(e) : null),
    };
  }
}

""");
    }

    [Test]
    public void UserClassNamespace_InstancePropertyOfUserClassNullableArrayType_GeneratesProperty()
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
                public UserClass[]? P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userclassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userclassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Initializer {
    P1: Array<UserClass | UserClass.Initializer> | null;
  }
  export interface Snapshot {
    P1: Array<UserClass.Snapshot> | null;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1 ? proxy.P1.map(e => UserClass.materialize(e)) : null,
    };
  }
}

""");
    }

    [Test]
    public void UserClassNamespace_InstancePropertyWithPrivateSetter_GeneratesNoInitializer()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public int P1 { get; private set; }
                public string P2 { get; private set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export namespace C1 {
  export interface Snapshot {
    P1: number;
    P2: string;
  }
  export function materialize(proxy: C1): C1.Snapshot {
    return {
      P1: proxy.P1,
      P2: proxy.P2,
    };
  }
}

""");
    }

    [Test]
    public void UserClassNamespace_WithoutExportedProperties_GeneratesNoShapes()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                private int P1 { get; set; }
                private string P2 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """

""");
    }

    [Test]
    public void UserClassNamespace_PropertyType_Func_DoesNotRenderInSnapshot()
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
            public Func<char, UserClass> P1 { get; set; }
        }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses[0], typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses[1], typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
        export namespace C1 {
          export interface Initializer {
            P1: (arg0: string) => UserClass;
          }
        }

        """);
    }
    
    [Test]
    public void UserClassNamespace_PropertyType_FuncAndInt_OmitsFuncInSnapshot()
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
            public Func<char, UserClass> P1 { get; set; }
            public int P2 { get; set; }
        }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses[0], typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses[1], typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassNamespaceRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
        export namespace C1 {
          export interface Initializer {
            P1: (arg0: string) => UserClass;
            P2: number;
          }
          export interface Snapshot {
            P2: number;
          }
          export function materialize(proxy: C1): C1.Snapshot {
            return {
              P2: proxy.P2,
            };
          }
        }

        """);
    }
}
