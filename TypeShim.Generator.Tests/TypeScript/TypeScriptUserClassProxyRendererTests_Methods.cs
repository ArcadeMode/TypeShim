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
    public void TypeScriptUserClassProxy_InstanceMethod_GeneratesSimpleFunction(string typeExpression, string typeScriptType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class C1
            {
                public {{typeExpression}} DoP1() {}
            }
        """.Replace("{{typeExpression}}", typeExpression));

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo]);
        TypescriptClassNameBuilder classNameBuilder = new(typeMapper);
        TypeScriptMethodRenderer methodRenderer = new(typeMapper);


        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, classNameBuilder).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript proxy class. Source class: N1.C1
class C1Proxy implements C1 {
  interop: WasmModuleExports;
  instance: object;

  constructor(instance: object, interop: WasmModuleExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public DoP1(): {{typeScriptType}} {
    return this.interop.N1.C1Interop.DoP1(this.instance);
  }

}
// Auto-generated TypeScript statics class. Source class: N1.C1
export class C1Statics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
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
            [TsExport]
            public class UserClass
            {
                public int Id { get; set; }
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class C1
            {
                public UserClass[] GetAll() {}
            }
        """);

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree, userClass]);
        List<INamedTypeSymbol> exportedClasses = [
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot()),
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(userClass), userClass.GetRoot()),
        ];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptClassNameBuilder classNameBuilder = new(typeMapper);
        TypeScriptMethodRenderer methodRenderer = new(typeMapper);


        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, classNameBuilder).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript proxy class. Source class: N1.C1
class C1Proxy implements C1 {
  interop: WasmModuleExports;
  instance: object;

  constructor(instance: object, interop: WasmModuleExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public GetAll(): Array<UserClass> {
    const res = this.interop.N1.C1Interop.GetAll(this.instance);
    return res.map(item => new UserClassProxy(item, this.interop));
  }

}
// Auto-generated TypeScript statics class. Source class: N1.C1
export class C1Statics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
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
            [TsExport]
            public class UserClass
            {
                public int Id { get; set; }
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class C1
            {
                public UserClass? GetMaybe() {}
            }
        """);

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree, userClass]);
        List<INamedTypeSymbol> exportedClasses = [
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot()),
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(userClass), userClass.GetRoot()),
        ];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptClassNameBuilder classNameBuilder = new(typeMapper);
        TypeScriptMethodRenderer methodRenderer = new(typeMapper);


        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, classNameBuilder).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript proxy class. Source class: N1.C1
class C1Proxy implements C1 {
  interop: WasmModuleExports;
  instance: object;

  constructor(instance: object, interop: WasmModuleExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public GetMaybe(): UserClass | null {
    const res = this.interop.N1.C1Interop.GetMaybe(this.instance);
    return res ? new UserClassProxy(res, this.interop) : null;
  }

}
// Auto-generated TypeScript statics class. Source class: N1.C1
export class C1Statics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
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
            [TsExport]
            public class UserClass
            {
                public int Id { get; set; }
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class C1
            {
                public void DoStuff(UserClass u) {}
            }
        """);

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree, userClass]);
        List<INamedTypeSymbol> exportedClasses = [
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot()),
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(userClass), userClass.GetRoot()),
        ];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptClassNameBuilder classNameBuilder = new(typeMapper);
        TypeScriptMethodRenderer methodRenderer = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, classNameBuilder).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript proxy class. Source class: N1.C1
class C1Proxy implements C1 {
  interop: WasmModuleExports;
  instance: object;

  constructor(instance: object, interop: WasmModuleExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public DoStuff(u: UserClass): void {
    const uInstance = u instanceof UserClassProxy ? u.instance : u;
    this.interop.N1.C1Interop.DoStuff(this.instance, uInstance);
  }

}
// Auto-generated TypeScript statics class. Source class: N1.C1
export class C1Statics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
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
            [TsExport]
            public class UserClass
            {
                public int Id { get; set; }
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class C1
            {
                public void DoStuff(UserClass? u) {}
            }
        """);

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree, userClass]);
        List<INamedTypeSymbol> exportedClasses = [
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot()),
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(userClass), userClass.GetRoot()),
        ];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptClassNameBuilder classNameBuilder = new(typeMapper);
        TypeScriptMethodRenderer methodRenderer = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, classNameBuilder).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript proxy class. Source class: N1.C1
class C1Proxy implements C1 {
  interop: WasmModuleExports;
  instance: object;

  constructor(instance: object, interop: WasmModuleExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public DoStuff(u: UserClass | null): void {
    const uInstance = u instanceof UserClassProxy ? u.instance : u;
    this.interop.N1.C1Interop.DoStuff(this.instance, uInstance);
  }

}
// Auto-generated TypeScript statics class. Source class: N1.C1
export class C1Statics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
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
            [TsExport]
            public class UserClass
            {
                public int Id { get; set; }
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class C1
            {
                public void DoStuff(UserClass[] u) {}
            }
        """);

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree, userClass]);
        List<INamedTypeSymbol> exportedClasses = [
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot()),
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(userClass), userClass.GetRoot()),
        ];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptClassNameBuilder classNameBuilder = new(typeMapper);
        TypeScriptMethodRenderer methodRenderer = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, classNameBuilder).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript proxy class. Source class: N1.C1
class C1Proxy implements C1 {
  interop: WasmModuleExports;
  instance: object;

  constructor(instance: object, interop: WasmModuleExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public DoStuff(u: Array<UserClass>): void {
    const uInstance = u.map(item => item instanceof UserClassProxy ? item.instance : item);
    this.interop.N1.C1Interop.DoStuff(this.instance, uInstance);
  }

}
// Auto-generated TypeScript statics class. Source class: N1.C1
export class C1Statics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
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
            [TsExport]
            public class UserClass
            {
                public int Id { get; set; }
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class C1
            {
                public void DoStuff(Task<UserClass> u) {}
            }
        """);

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree, userClass]);
        List<INamedTypeSymbol> exportedClasses = [
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot()),
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(userClass), userClass.GetRoot()),
        ];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptClassNameBuilder classNameBuilder = new(typeMapper);
        TypeScriptMethodRenderer methodRenderer = new(typeMapper);

        string interopClass = new TypescriptUserClassProxyRenderer(classInfo, methodRenderer, classNameBuilder).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript proxy class. Source class: N1.C1
class C1Proxy implements C1 {
  interop: WasmModuleExports;
  instance: object;

  constructor(instance: object, interop: WasmModuleExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public DoStuff(u: Promise<UserClass>): void {
    const uInstance = u.then(item => item instanceof UserClassProxy ? item.instance : item);
    this.interop.N1.C1Interop.DoStuff(this.instance, uInstance);
  }

}
// Auto-generated TypeScript statics class. Source class: N1.C1
export class C1Statics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
  }

}

"""));
    }
}
