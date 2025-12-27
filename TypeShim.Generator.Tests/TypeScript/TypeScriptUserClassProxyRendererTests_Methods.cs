using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework.Internal;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassProxyRendererTests_Methods
{
    [TestCase("string", "string")]
    [TestCase("double", "number")]
    [TestCase("bool", "boolean")]
    public void TypeScriptUserClassProxy_InstanceMethod_GeneratesSimpleFunction(string typeExpression, string typeScriptType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public {{typeExpression}} DoP1() {}
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);


        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public DoP1(): {{typeScriptType}} {
    return TypeShimConfig.exports.N1.C1Interop.DoP1(this.instance);
  }

}

""".Replace("{{typeScriptType}}", typeScriptType)));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceMethod_WithUserClassReturnType_GeneratesArrayProxies()
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
                public UserClass[] GetAll() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);


        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public GetAll(): Array<UserClass.Proxy> {
    const res = TypeShimConfig.exports.N1.C1Interop.GetAll(this.instance);
    return res.map(e => ProxyBase.fromHandle(UserClass.Proxy, e));
  }

}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceMethod_WithNullableUserClassReturnType_GeneratesArrayProxies()
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
                public UserClass? GetMaybe() {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);


        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public GetMaybe(): UserClass.Proxy | null {
    const res = TypeShimConfig.exports.N1.C1Interop.GetMaybe(this.instance);
    return res ? ProxyBase.fromHandle(UserClass.Proxy, res) : null;
  }

}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceMethod_WithUserClassParameterType_ExtractsInstanceProperty()
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
                public void DoStuff(UserClass u) {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public DoStuff(u: UserClass.Proxy | UserClass.Snapshot): void {
    const uInstance = u instanceof UserClass.Proxy ? u.instance : u;
    TypeShimConfig.exports.N1.C1Interop.DoStuff(this.instance, uInstance);
  }

}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceMethod_WithMultipleUserClassParameterType_GeneratesAllProxySnapshotPermutations()
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
                public void DoStuff(UserClass u, UserClass v) {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public DoStuff(u: UserClass.Proxy | UserClass.Snapshot, v: UserClass.Proxy | UserClass.Snapshot): void {
    const uInstance = u instanceof UserClass.Proxy ? u.instance : u;
    const vInstance = v instanceof UserClass.Proxy ? v.instance : v;
    TypeShimConfig.exports.N1.C1Interop.DoStuff(this.instance, uInstance, vInstance);
  }

}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceMethod_WithNullableUserClassParameterType_ExtractsInstanceProperty()
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
                public void DoStuff(UserClass? u) {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public DoStuff(u: UserClass.Proxy | UserClass.Snapshot | null): void {
    const uInstance = u ? u instanceof UserClass.Proxy ? u.instance : u : null;
    TypeShimConfig.exports.N1.C1Interop.DoStuff(this.instance, uInstance);
  }

}

"""));
    }

    [Test]
    public void TypeScriptUserClassProxy_InstanceMethod_WithUserClassArrayParameterType_ExtractsInstanceProperty()
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
                public void DoStuff(UserClass[] u) {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public DoStuff(u: Array<UserClass.Proxy | UserClass.Snapshot>): void {
    const uInstance = u.map(e => e instanceof UserClass.Proxy ? e.instance : e);
    TypeShimConfig.exports.N1.C1Interop.DoStuff(this.instance, uInstance);
  }

}

"""));
    }
    [Test]
    public void TypeScriptUserClassProxy_InstanceMethod_WithUserClassTaskParameterType_ExtractsInstanceProperty()
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
                public void DoStuff(Task<UserClass> u) {}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy extends ProxyBase {
  constructor() {
    super(null!);
  }

  public DoStuff(u: Promise<UserClass.Proxy | UserClass.Snapshot>): void {
    const uInstance = u.then(e => e instanceof UserClass.Proxy ? e.instance : e);
    TypeShimConfig.exports.N1.C1Interop.DoStuff(this.instance, uInstance);
  }

}

"""));
    }
}
