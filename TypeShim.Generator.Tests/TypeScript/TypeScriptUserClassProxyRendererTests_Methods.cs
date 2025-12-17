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
        TypeScriptMethodRenderer methodRenderer = new(symbolNameProvider);


        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public DoP1(): {{typeScriptType}} {
    return this.interop.N1.C1Interop.DoP1(this.instance);
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
        TypeScriptMethodRenderer methodRenderer = new(symbolNameProvider);


        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public GetAll(): Array<UserClass.Proxy> {
    const res = this.interop.N1.C1Interop.GetAll(this.instance);
    return res.map(e => new UserClass.Proxy(e, this.interop));
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
        TypeScriptMethodRenderer methodRenderer = new(symbolNameProvider);


        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public GetMaybe(): UserClass.Proxy | null {
    const res = this.interop.N1.C1Interop.GetMaybe(this.instance);
    return res ? new UserClass.Proxy(res, this.interop) : null;
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
        TypeScriptMethodRenderer methodRenderer = new(symbolNameProvider);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public DoStuff(u: UserClass.Proxy | UserClass.Snapshot): void {
    if (u instanceof UserClass.Proxy) {
      const uInstance = u.instance;
      this.interop.N1.C1Interop.DoStuff(this.instance, uInstance);
    } else if (u instanceof UserClass.Snapshot) {
      this.interop.N1.C1Interop.DoStuff_1(this.instance, u);
    } else {
      throw new Error("No overload for interop method 'DoStuff' matches the provided arguments.");
    }
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
        TypeScriptMethodRenderer methodRenderer = new(symbolNameProvider);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public DoStuff(u: UserClass.Proxy | UserClass.Snapshot, v: UserClass.Proxy | UserClass.Snapshot): void {
    if (u instanceof UserClass.Proxy && v instanceof UserClass.Proxy) {
      const uInstance = u.instance;
      const vInstance = v.instance;
      this.interop.N1.C1Interop.DoStuff(this.instance, uInstance, vInstance);
    } else if (u instanceof UserClass.Snapshot && v instanceof UserClass.Proxy) {
      const vInstance = v.instance;
      this.interop.N1.C1Interop.DoStuff_1(this.instance, u, vInstance);
    } else if (u instanceof UserClass.Proxy && v instanceof UserClass.Snapshot) {
      const uInstance = u.instance;
      this.interop.N1.C1Interop.DoStuff_2(this.instance, uInstance, v);
    } else if (u instanceof UserClass.Snapshot && v instanceof UserClass.Snapshot) {
      this.interop.N1.C1Interop.DoStuff_12(this.instance, u, v);
    } else {
      throw new Error("No overload for interop method 'DoStuff' matches the provided arguments.");
    }
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
        TypeScriptMethodRenderer methodRenderer = new(symbolNameProvider);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public DoStuff(u: UserClass.Proxy | UserClass.Snapshot | null): void {
    if (u instanceof UserClass.Proxy) {
      const uInstance = u.instance;
      this.interop.N1.C1Interop.DoStuff(this.instance, uInstance);
    } else if (u instanceof UserClass.Snapshot) {
      this.interop.N1.C1Interop.DoStuff_1(this.instance, u);
    } else {
      throw new Error("No overload for interop method 'DoStuff' matches the provided arguments.");
    }
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
        TypeScriptMethodRenderer methodRenderer = new(symbolNameProvider);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public DoStuff(u: Array<UserClass.Proxy | UserClass.Snapshot>): void {
    if (u.every(e => e instanceof UserClass.Proxy)) {
      const uInstance = u.map(e => e.instance);
      this.interop.N1.C1Interop.DoStuff(this.instance, uInstance);
    } else if (u.every(e => e instanceof UserClass.Snapshot)) {
      this.interop.N1.C1Interop.DoStuff_1(this.instance, u);
    } else {
      throw new Error("No overload for interop method 'DoStuff' matches the provided arguments.");
    }
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
        TypeScriptMethodRenderer methodRenderer = new(symbolNameProvider);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, symbolNameProvider).Render(0);

        Assert.That(interopClass, Is.EqualTo("""    
export class Proxy {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public DoStuff(u: Promise<UserClass.Proxy | UserClass.Snapshot>): void {
    const uInstance = u.then(e => e instanceof UserClass.Proxy ? item.instance : item);
    this.interop.N1.C1Interop.DoStuff(this.instance, uInstance);
  }

}

"""));
    }
}
