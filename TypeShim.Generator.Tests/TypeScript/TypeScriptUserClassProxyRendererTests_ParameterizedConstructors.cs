using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassProxyRendererTests_ParameterizedConstructors
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

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(p1: {{typeScriptType}}) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1));
  }

  public get P1(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }
}

""".Replace("{{typeScriptType}}", typeScriptType));
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

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(p1: {{typeScriptType}}) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1));
  }

  public get P1(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }
}

""".Replace("{{typeScriptType}}", typeScriptType));
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();
        // this type of constructor might be used as a copy constructor.
        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(p1: UserClass | UserClass.Initializer) {
    const p1Instance = p1 instanceof UserClass ? p1.instance : p1;
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1Instance));
  }

}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithNullableUserClassParameterType()
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
            public class C1(UserClass? p1)
            {
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();
        // this type of constructor might be used as a copy constructor.
        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(p1: UserClass | UserClass.Initializer | null) {
    const p1Instance = p1 ? p1 instanceof UserClass ? p1.instance : p1 : null;
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1Instance));
  }

}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithUserClassArrayParameterType()
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
            public class C1(UserClass[] p1)
            {
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();
        // this type of constructor might be used as a copy constructor.
        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(p1: Array<UserClass | UserClass.Initializer>) {
    const p1Instance = p1.map(e => e instanceof UserClass ? e.instance : e);
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1Instance));
  }

}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithNullableUserClassArrayParameterType()
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
            public class C1(UserClass?[] p1)
            {
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();
        // this type of constructor might be used as a copy constructor.
        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(p1: Array<UserClass | UserClass.Initializer | null>) {
    const p1Instance = p1.map(e => e ? e instanceof UserClass ? e.instance : e : null);
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1Instance));
  }

}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithUserClassNullableArrayParameterType()
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
            public class C1(UserClass[]? p1)
            {
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();
        // this type of constructor might be used as a copy constructor.
        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(p1: Array<UserClass | UserClass.Initializer> | null) {
    const p1Instance = p1 ? p1.map(e => e instanceof UserClass ? e.instance : e) : null;
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1Instance));
  }

}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithNullableUserClassNullableArrayParameterType()
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
            public class C1(UserClass?[]? p1)
            {
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();
        // this type of constructor might be used as a copy constructor.
        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(p1: Array<UserClass | UserClass.Initializer | null> | null) {
    const p1Instance = p1 ? p1.map(e => e ? e instanceof UserClass ? e.instance : e : null) : null;
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1Instance));
  }

}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithUserClassTaskParameterType()
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
            public class C1(Task<UserClass> p1)
            {
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();
        // this type of constructor might be used as a copy constructor.
        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(p1: Promise<UserClass | UserClass.Initializer>) {
    const p1Instance = p1.then(e => e instanceof UserClass ? e.instance : e);
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1Instance));
  }

}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithNullableUserClassTaskParameterType()
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
            public class C1(Task<UserClass?> p1)
            {
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();
        // this type of constructor might be used as a copy constructor.
        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(p1: Promise<UserClass | UserClass.Initializer | null>) {
    const p1Instance = p1.then(e => e ? e instanceof UserClass ? e.instance : e : null);
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1Instance));
  }

}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithUserClassNullableTaskParameterType()
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
            public class C1(Task<UserClass>? p1)
            {
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();
        // this type of constructor might be used as a copy constructor.
        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(p1: Promise<UserClass | UserClass.Initializer> | null) {
    const p1Instance = p1 ? p1.then(e => e instanceof UserClass ? e.instance : e) : null;
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1Instance));
  }

}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_WithNullableUserClassNullableTaskParameterType()
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
            public class C1(Task<UserClass?>? p1)
            {
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();
        // this type of constructor might be used as a copy constructor.
        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(p1: Promise<UserClass | UserClass.Initializer | null> | null) {
    const p1Instance = p1 ? p1.then(e => e ? e instanceof UserClass ? e.instance : e : null) : null;
    super(TypeShimConfig.exports.N1.C1Interop.ctor(p1Instance));
  }

}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterlessConstructor_AndUnexportedPropertyType()
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
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """    
export class C1 extends ProxyBase {
  constructor(jsObject: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor(jsObject));
  }

  public get P1(): ManagedObject {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }

  public set P1(value: ManagedObject) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_ParameterizedConstructor_AndUnexportedPropertyType()
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
        new TypescriptUserClassProxyRenderer(symbolNameProvider, renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 extends ProxyBase {
  constructor(i: number, jsObject: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor(i, jsObject));
  }

  public get P1(): ManagedObject {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }

  public set P1(value: ManagedObject) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value);
  }
}

""");
    }
}
