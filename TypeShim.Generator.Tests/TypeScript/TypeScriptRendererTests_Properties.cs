using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

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

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);


        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public get P1(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
  }

  public set P1(value: {{typeScriptType}}) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, value);
  }

}

""".Replace("{{typeScriptType}}", typeScriptType)));
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

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public static get P1(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1Interop.get_P1();
  }

  public static set P1(value: {{typeScriptType}}) {
    TypeShimConfig.exports.N1.C1Interop.set_P1(value);
  }

}

""".Replace("{{typeScriptType}}", typeScriptType)));
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

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        ModuleInfo moduleInfo = new()
        {
            ExportedClasses = [classInfo],
            HierarchyInfo = ModuleHierarchyInfo.FromClasses([classInfo], symbolNameProvider),
        };

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""
export class Proxy {
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

""".Replace("{{typeScriptType}}", typeScriptType)));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceParameter_WithUserClassParameterType_RendersConversionsCorrectly()
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
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public get P1(): UserClass.Proxy {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return ProxyBase.fromHandle(UserClass.Proxy, res);
  }

  public set P1(value: UserClass.Proxy | UserClass.Snapshot) {
    const valueInstance = value instanceof UserClass.Proxy ? value.instance : value;
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, valueInstance);
  }

}

"""));
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

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""
export class Proxy {
  private constructor() {}

  public static get P1(): UserClass.Proxy {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1();
    return ProxyBase.fromHandle(UserClass.Proxy, res);
  }

  public static set P1(value: UserClass.Proxy | UserClass.Snapshot) {
    const valueInstance = value instanceof UserClass.Proxy ? value.instance : value;
    TypeShimConfig.exports.N1.C1Interop.set_P1(valueInstance);
  }

}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceParameter_WithNullableUserClassParameterType_RendersConversionsCorrectly()
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
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public get P1(): UserClass.Proxy | null {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res ? ProxyBase.fromHandle(UserClass.Proxy, res) : null;
  }

  public set P1(value: UserClass.Proxy | UserClass.Snapshot | null) {
    const valueInstance = value ? value instanceof UserClass.Proxy ? value.instance : value : null;
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, valueInstance);
  }

}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceParameter_WithUserClassTaskParameterType_RendersConversionsCorrectly()
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
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public get P1(): Promise<UserClass.Proxy> {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res.then(e => ProxyBase.fromHandle(UserClass.Proxy, e));
  }

  public set P1(value: Promise<UserClass.Proxy | UserClass.Snapshot>) {
    const valueInstance = value.then(e => e instanceof UserClass.Proxy ? e.instance : e);
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, valueInstance);
  }

}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceParameter_WithNullableUserClassTaskParameterType_RendersConversionsCorrectly()
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

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public get P1(): Promise<UserClass.Proxy | null> {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res.then(e => e ? ProxyBase.fromHandle(UserClass.Proxy, e) : null);
  }

  public set P1(value: Promise<UserClass.Proxy | UserClass.Snapshot | null>) {
    const valueInstance = value.then(e => e ? e instanceof UserClass.Proxy ? e.instance : e : null);
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, valueInstance);
  }

}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceParameter_WithUserClassArrayParameterType_RendersConversionsCorrectly()
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
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, typeCache).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public get P1(): Array<UserClass.Proxy> {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res.map(e => ProxyBase.fromHandle(UserClass.Proxy, e));
  }

  public set P1(value: Array<UserClass.Proxy | UserClass.Snapshot>) {
    const valueInstance = value.map(e => e instanceof UserClass.Proxy ? e.instance : e);
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, valueInstance);
  }

}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceParameter_WithNullableUserClassArrayParameterType_RendersConversionsCorrectly()
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

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public get P1(): Array<UserClass.Proxy | null> {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res.map(e => e ? ProxyBase.fromHandle(UserClass.Proxy, e) : null);
  }

  public set P1(value: Array<UserClass.Proxy | UserClass.Snapshot | null>) {
    const valueInstance = value.map(e => e ? e instanceof UserClass.Proxy ? e.instance : e : null);
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, valueInstance);
  }

}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceParameter_WithUserClassNullableArrayParameterType_RendersConversionsCorrectly()
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

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public get P1(): Array<UserClass.Proxy> | null {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res ? res.map(e => ProxyBase.fromHandle(UserClass.Proxy, e)) : null;
  }

  public set P1(value: Array<UserClass.Proxy | UserClass.Snapshot> | null) {
    const valueInstance = value ? value.map(e => e instanceof UserClass.Proxy ? e.instance : e) : null;
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, valueInstance);
  }

}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceParameter_WithNullableUserClassNullableArrayParameterType_RendersConversionsCorrectly()
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

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public get P1(): Array<UserClass.Proxy | null> | null {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res ? res.map(e => e ? ProxyBase.fromHandle(UserClass.Proxy, e) : null) : null;
  }

  public set P1(value: Array<UserClass.Proxy | UserClass.Snapshot | null> | null) {
    const valueInstance = value ? value.map(e => e ? e instanceof UserClass.Proxy ? e.instance : e : null) : null;
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, valueInstance);
  }

}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceParameter_WithNullableUserClassNullableTaskParameterType_RendersConversionsCorrectly()
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

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public get P1(): Promise<UserClass.Proxy | null> | null {
    const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
    return res ? res.then(e => e ? ProxyBase.fromHandle(UserClass.Proxy, e) : null) : null;
  }

  public set P1(value: Promise<UserClass.Proxy | UserClass.Snapshot | null> | null) {
    const valueInstance = value ? value.then(e => e ? e instanceof UserClass.Proxy ? e.instance : e : null) : null;
    TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, valueInstance);
  }

}

"""));
    }
}
