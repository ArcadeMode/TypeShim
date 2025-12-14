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

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
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

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree, userClass]);
        List<INamedTypeSymbol> exportedClasses = [
            ..TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot()),
            ..TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(userClass), userClass.GetRoot()),
        ];
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
    return res.map(item => new UserClass.Proxy(item, this.interop));
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

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree, userClass]);
        List<INamedTypeSymbol> exportedClasses = [
            ..TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot()),
            ..TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(userClass), userClass.GetRoot()),
        ];
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

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree, userClass]);
        List<INamedTypeSymbol> exportedClasses = [
            ..TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot()),
            ..TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(userClass), userClass.GetRoot()),
        ];
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

  public DoStuff(u: UserClass.Proxy): void {
    const uInstance = u instanceof UserClass.Proxy ? u.instance : u;
    this.interop.N1.C1Interop.DoStuff(this.instance, uInstance);
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

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree, userClass]);
        List<INamedTypeSymbol> exportedClasses = [
            ..TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot()),
            ..TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(userClass), userClass.GetRoot()),
        ];
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

  public DoStuff(u: UserClass.Proxy | null): void {
    const uInstance = u instanceof UserClass.Proxy ? u.instance : u;
    this.interop.N1.C1Interop.DoStuff(this.instance, uInstance);
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

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree, userClass]);
        List<INamedTypeSymbol> exportedClasses = [
            ..TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot()),
            ..TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(userClass), userClass.GetRoot()),
        ];
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

  public DoStuff(u: Array<UserClass.Proxy>): void {
    const uInstance = u.map(item => item instanceof UserClass.Proxy ? item.instance : item);
    this.interop.N1.C1Interop.DoStuff(this.instance, uInstance);
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

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree, userClass]);
        List<INamedTypeSymbol> exportedClasses = [
            ..TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot()),
            ..TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(userClass), userClass.GetRoot()),
        ];
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

  public DoStuff(u: Promise<UserClass.Proxy>): void {
    const uInstance = u.then(item => item instanceof UserClass.Proxy ? item.instance : item);
    this.interop.N1.C1Interop.DoStuff(this.instance, uInstance);
  }

}

"""));
    }
}
