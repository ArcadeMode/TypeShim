using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassSnapshotRendererTests
{
    [TestCase("string", "string")]
    [TestCase("double", "number")]
    [TestCase("bool", "boolean")]
    public void TypeScriptUserClassSnapshot_InstancePropertyOfSimpleType_GeneratesProperty(string typeExpression, string typeScriptType)
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

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassSnapshotRenderer(symbolNameProvider, renderContext).Render();

        Assert.That(renderContext.ToString(), Is.EqualTo("""    
export interface Snapshot {
  P1: {{typeScriptType}};
}
export const Snapshot: {
  [Symbol.hasInstance](v: unknown): boolean;
} = {
  [Symbol.hasInstance](v: unknown) {
    if (!v || typeof v !== 'object') return false;
    const o = v as any;
    return typeof o.P1 === '{{typeScriptType}}';
  }
};
export function snapshot(proxy: C1.Proxy): C1.Snapshot {
  return {
    P1: proxy.P1,
  };
}

""".Replace("{{typeScriptType}}", typeScriptType)));
    }

    [TestCase("string[]", "Array<string>", "string")]
    [TestCase("double[]", "Array<number>", "number")]
    public void TypeScriptUserClassSnapshot_InstancePropertyOfArrayType_GeneratesProperty(string typeExpression, string typeScriptType, string typeScriptElementType)
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

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassSnapshotRenderer(symbolNameProvider, renderContext).Render();

        Assert.That(renderContext.ToString(), Is.EqualTo("""    
export interface Snapshot {
  P1: {{typeScriptType}};
}
export const Snapshot: {
  [Symbol.hasInstance](v: unknown): boolean;
} = {
  [Symbol.hasInstance](v: unknown) {
    if (!v || typeof v !== 'object') return false;
    const o = v as any;
    return Array.isArray(o.P1) && o.P1.every((e: any) => typeof e === '{{typeScriptElementType}}');
  }
};
export function snapshot(proxy: C1.Proxy): C1.Snapshot {
  return {
    P1: proxy.P1,
  };
}

""".Replace("{{typeScriptType}}", typeScriptType).Replace("{{typeScriptElementType}}", typeScriptElementType)));
    }

    [TestCase("Task<string>", "Promise<string>")]
    [TestCase("Task<double>", "Promise<number>")]
    public void TypeScriptUserClassSnapshot_InstancePropertyOfTaskType_GeneratesPromiseProperty(string csTypeExpression, string tsTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public {{csTypeExpression}} P1 { get; set; }
            }
        """.Replace("{{csTypeExpression}}", csTypeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassSnapshotRenderer(symbolNameProvider, renderContext).Render();

        Assert.That(renderContext.ToString(), Is.EqualTo("""
export interface Snapshot {
  P1: {{tsTypeExpression}};
}
export const Snapshot: {
  [Symbol.hasInstance](v: unknown): boolean;
} = {
  [Symbol.hasInstance](v: unknown) {
    if (!v || typeof v !== 'object') return false;
    const o = v as any;
    return (o.P1 !== null && typeof (o.P1 as any).then === 'function');
  }
};
export function snapshot(proxy: C1.Proxy): C1.Snapshot {
  return {
    P1: proxy.P1,
  };
}

""".Replace("{{tsTypeExpression}}", tsTypeExpression)));
    }

    [Test]
    public void TypeScriptUserClassSnapshot_InstancePropertyOfUserClassType_GeneratesProperty()
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

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userclassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userclassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo, userclassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassSnapshotRenderer(symbolNameProvider, renderContext).Render();

        Assert.That(renderContext.ToString(), Is.EqualTo("""    
export interface Snapshot {
  P1: UserClass.Snapshot;
}
export const Snapshot: {
  [Symbol.hasInstance](v: unknown): boolean;
} = {
  [Symbol.hasInstance](v: unknown) {
    if (!v || typeof v !== 'object') return false;
    const o = v as any;
    return o.P1 instanceof UserClass.Snapshot;
  }
};
export function snapshot(proxy: C1.Proxy): C1.Snapshot {
  return {
    P1: UserClass.snapshot(proxy.P1),
  };
}

"""));
    }

    [Test]
    public void TypeScriptUserClassSnapshot_InstancePropertyOfNullableUserClassType_GeneratesProperty()
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

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userclassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userclassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo, userclassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassSnapshotRenderer(symbolNameProvider, renderContext).Render();

        Assert.That(renderContext.ToString(), Is.EqualTo("""    
export interface Snapshot {
  P1: UserClass.Snapshot | null;
}
export const Snapshot: {
  [Symbol.hasInstance](v: unknown): boolean;
} = {
  [Symbol.hasInstance](v: unknown) {
    if (!v || typeof v !== 'object') return false;
    const o = v as any;
    return (o.P1 === null || o.P1 instanceof UserClass.Snapshot);
  }
};
export function snapshot(proxy: C1.Proxy): C1.Snapshot {
  return {
    P1: proxy.P1 ? UserClass.snapshot(proxy.P1) : null,
  };
}

"""));
    }

    [Test]
    public void TypeScriptUserClassSnapshot_InstancePropertyOfUserClassTaskType_GeneratesProperty()
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

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userclassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userclassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo, userclassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassSnapshotRenderer(symbolNameProvider, renderContext).Render();

        Assert.That(renderContext.ToString(), Is.EqualTo("""    
export interface Snapshot {
  P1: Promise<UserClass.Snapshot>;
}
export const Snapshot: {
  [Symbol.hasInstance](v: unknown): boolean;
} = {
  [Symbol.hasInstance](v: unknown) {
    if (!v || typeof v !== 'object') return false;
    const o = v as any;
    return (o.P1 !== null && typeof (o.P1 as any).then === 'function');
  }
};
export function snapshot(proxy: C1.Proxy): C1.Snapshot {
  return {
    P1: proxy.P1.then(e => UserClass.snapshot(e)),
  };
}

"""));
    }

    [Test]
    public void TypeScriptUserClassSnapshot_InstancePropertyOfUserClassArrayType_GeneratesProperty()
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

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userclassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userclassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        RenderContext renderContext = new(classInfo, [classInfo, userclassInfo], RenderOptions.TypeScript);
        new TypeScriptUserClassSnapshotRenderer(symbolNameProvider, renderContext).Render();

        Assert.That(renderContext.ToString(), Is.EqualTo("""    
export interface Snapshot {
  P1: Array<UserClass.Snapshot>;
}
export const Snapshot: {
  [Symbol.hasInstance](v: unknown): boolean;
} = {
  [Symbol.hasInstance](v: unknown) {
    if (!v || typeof v !== 'object') return false;
    const o = v as any;
    return Array.isArray(o.P1) && o.P1.every((e: any) => e instanceof UserClass.Snapshot);
  }
};
export function snapshot(proxy: C1.Proxy): C1.Snapshot {
  return {
    P1: proxy.P1.map(e => UserClass.snapshot(e)),
  };
}

"""));
    }
}
