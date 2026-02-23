using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassProxyRendererTests_Properties
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

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
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

    [TestCase("string", "string")]
    [TestCase("double", "number")]
    [TestCase("bool", "boolean")]
    public void TypeScriptUserClassProxy_InstanceProperty_GeneratesGetFunction(string typeExpression, string typeScriptType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public {{typeExpression}} P1 { get; init; }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
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
}

""".Replace("{{typeScriptType}}", typeScriptType));
    }

    [TestCase("string", "string")]
    [TestCase("double", "number")]
    [TestCase("bool", "boolean")]
    public void TypeScriptUserClassProxy_NonStaticClass_StaticProperty_GeneratesGetAndSetFunctions(string typeExpression, string typeScriptType)
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

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
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

  public static get P1(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1Interop.get_P1();
  }

  public static set P1(value: {{typeScriptType}}) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(value);
  }
}

""".Replace("{{typeScriptType}}", typeScriptType));
    }

    [TestCase("string", "string")]
    [TestCase("double", "number")]
    [TestCase("bool", "boolean")]
    public void TypeScriptUserClassProxy_StaticProperty_GeneratesGetAndSetFunctions(string typeExpression, string typeScriptType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public static class C1
            {
                public static {{typeExpression}} P1 { get; set; }
                public static {{typeExpression}} P2 { get; init; }
                public static {{typeExpression}} P3 { get; }
                public static {{typeExpression}} P4 => 1
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 {
  private constructor() {}

  public static get P1(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1Interop.get_P1();
  }

  public static set P1(value: {{typeScriptType}}) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(value);
  }

  public static get P2(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1Interop.get_P2();
  }

  public static set P2(value: {{typeScriptType}}) {
    TypeShimConfig.exports.N1.C1Interop.set_P2(value);
  }

  public static get P3(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1Interop.get_P3();
  }

  public static get P4(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1Interop.get_P4();
  }
}

""".Replace("{{typeScriptType}}", typeScriptType));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithUserClassParameterType_RendersConversionsCorrectly()
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
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

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
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_StaticParameter_WithUserClassParameterType_RendersConversionsCorrectly()
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
            public static class C1
            {
                public static UserClass P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
export class C1 {
  private constructor() {}

  public static get P1(): UserClass {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1();
    return ProxyBase.fromHandle(UserClass, res);
  }

  public static set P1(value: UserClass | UserClass.Initializer) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(value instanceof UserClass ? value.instance : value);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithNullableUserClassParameterType_RendersConversionsCorrectly()
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
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """    
export class C1 extends ProxyBase {
  constructor(jsObject: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: jsObject.P1 ? jsObject.P1 instanceof UserClass ? jsObject.P1.instance : jsObject.P1 : null }));
  }

  public get P1(): UserClass | null {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res ? ProxyBase.fromHandle(UserClass, res) : null;
  }

  public set P1(value: UserClass | UserClass.Initializer | null) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value ? value instanceof UserClass ? value.instance : value : null);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithUserClassTaskParameterType_RendersConversionsCorrectly()
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
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """    
export class C1 extends ProxyBase {
  constructor(jsObject: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: jsObject.P1.then(e => e instanceof UserClass ? e.instance : e) }));
  }

  public get P1(): Promise<UserClass> {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res.then(e => ProxyBase.fromHandle(UserClass, e));
  }

  public set P1(value: Promise<UserClass | UserClass.Initializer>) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value.then(e => e instanceof UserClass ? e.instance : e));
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithVoidTaskType()
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
                public Task P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """    
export class C1 extends ProxyBase {
  constructor(jsObject: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject }));
  }

  public get P1(): Promise<void> {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }

  public set P1(value: Promise<void>) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithUserClassArrayParameterType_RendersConversionsCorrectly()
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
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """    
export class C1 extends ProxyBase {
  constructor(jsObject: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: jsObject.P1.map(e => e instanceof UserClass ? e.instance : e) }));
  }

  public get P1(): Array<UserClass> {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res.map(e => ProxyBase.fromHandle(UserClass, e));
  }

  public set P1(value: Array<UserClass | UserClass.Initializer>) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value.map(e => e instanceof UserClass ? e.instance : e));
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithNullableUserClassArrayParameterType_RendersConversionsCorrectly()
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
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """    
export class C1 extends ProxyBase {
  constructor(jsObject: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: jsObject.P1.map(e => e ? e instanceof UserClass ? e.instance : e : null) }));
  }

  public get P1(): Array<UserClass | null> {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res.map(e => e ? ProxyBase.fromHandle(UserClass, e) : null);
  }

  public set P1(value: Array<UserClass | UserClass.Initializer | null>) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value.map(e => e ? e instanceof UserClass ? e.instance : e : null));
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithUserClassNullableArrayParameterType_RendersConversionsCorrectly()
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
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """    
export class C1 extends ProxyBase {
  constructor(jsObject: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: jsObject.P1 ? jsObject.P1.map(e => e instanceof UserClass ? e.instance : e) : null }));
  }

  public get P1(): Array<UserClass> | null {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res ? res.map(e => ProxyBase.fromHandle(UserClass, e)) : null;
  }

  public set P1(value: Array<UserClass | UserClass.Initializer> | null) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value ? value.map(e => e instanceof UserClass ? e.instance : e) : null);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithNullableUserClassNullableArrayParameterType_RendersConversionsCorrectly()
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
                public UserClass?[]? P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """    
export class C1 extends ProxyBase {
  constructor(jsObject: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: jsObject.P1 ? jsObject.P1.map(e => e ? e instanceof UserClass ? e.instance : e : null) : null }));
  }

  public get P1(): Array<UserClass | null> | null {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res ? res.map(e => e ? ProxyBase.fromHandle(UserClass, e) : null) : null;
  }

  public set P1(value: Array<UserClass | UserClass.Initializer | null> | null) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value ? value.map(e => e ? e instanceof UserClass ? e.instance : e : null) : null);
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithNullableUserClassTaskParameterType_RendersConversionsCorrectly()
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
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """    
export class C1 extends ProxyBase {
  constructor(jsObject: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: jsObject.P1.then(e => e ? e instanceof UserClass ? e.instance : e : null) }));
  }

  public get P1(): Promise<UserClass | null> {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res.then(e => e ? ProxyBase.fromHandle(UserClass, e) : null);
  }

  public set P1(value: Promise<UserClass | UserClass.Initializer | null>) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value.then(e => e ? e instanceof UserClass ? e.instance : e : null));
  }
}

""");
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceProperty_WithNullableUserClassNullableTaskParameterType_RendersConversionsCorrectly()
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
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """    
export class C1 extends ProxyBase {
  constructor(jsObject: C1.Initializer) {
    super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: jsObject.P1 ? jsObject.P1.then(e => e ? e instanceof UserClass ? e.instance : e : null) : null }));
  }

  public get P1(): Promise<UserClass | null> | null {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res ? res.then(e => e ? ProxyBase.fromHandle(UserClass, e) : null) : null;
  }

  public set P1(value: Promise<UserClass | UserClass.Initializer | null> | null) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value ? value.then(e => e ? e instanceof UserClass ? e.instance : e : null) : null);
  }
}

""");
    }
}
