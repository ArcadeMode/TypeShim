using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework.Internal;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptUserClassProxyRendererTests_Delegates
{
    [Test]
    public void TypeScriptUserClassProxy_MethodWithAction0ParameterType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class C1
        {
            public void M1(Action callback) => callback();
        }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
        export class C1 extends ProxyBase {
          constructor() {
            super(TypeShimConfig.exports.N1.C1Interop.ctor());
          }

          public M1(callback: () => void): void {
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, callback);
          }
        }

        """);
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithUserClassParameterType_NonInitializable()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class UserClass
        {
            private UserClass() {}
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
            public void M1(Action<UserClass> callback) => callback(new UserClass() { Id = 1 });
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
        export class C1 extends ProxyBase {
          constructor() {
            super(TypeShimConfig.exports.N1.C1Interop.ctor());
          }

          public M1(callback: (arg0: UserClass) => void): void {
            const callbackInstance = (arg0: ManagedObject) => callback(ProxyBase.fromHandle(UserClass, arg0));
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, callbackInstance);
          }
        }

        """);
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithUserClassParameterType_Initializable()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class UserClass
        {
            public UserClass() {}
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
            public void M1(Action<UserClass> callback) => callback(new UserClass() { Id = 1 });
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
        export class C1 extends ProxyBase {
          constructor() {
            super(TypeShimConfig.exports.N1.C1Interop.ctor());
          }

          public M1(callback: (arg0: UserClass) => void): void {
            const callbackInstance = (arg0: ManagedObject) => callback(ProxyBase.fromHandle(UserClass, arg0));
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, callbackInstance);
          }
        }

        """);
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithUserClassAndPrimitiveParameterTypes()
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
            public void M1(Action<UserClass, int> callback) => callback(new UserClass() { Id = 1 }, 2);
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

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
        export class C1 extends ProxyBase {
          constructor() {
            super(TypeShimConfig.exports.N1.C1Interop.ctor());
          }

          public M1(callback: (arg0: UserClass, arg1: number) => void): void {
            const callbackInstance = (arg0: ManagedObject, arg1: number) => callback(ProxyBase.fromHandle(UserClass, arg0), arg1);
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, callbackInstance);
          }
        }

        """);
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithMultipleUserClassAndPrimitiveParameterTypes()
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
        
        SyntaxTree anotherUserClass = CSharpSyntaxTree.ParseText("""
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class AnotherUserClass
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
            public void M1(Action<UserClass, int, AnotherUserClass> callback) => callback(new UserClass() { Id = 1 }, 2, new AnotherUserClass { Id = 3 });
        }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass), CSharpFileInfo.Create(anotherUserClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(3));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses[0], typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses[1], typeCache).Build();
        ClassInfo anotherUserClassInfo = new ClassInfoBuilder(exportedClasses[2], typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo, anotherUserClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
        export class C1 extends ProxyBase {
          constructor() {
            super(TypeShimConfig.exports.N1.C1Interop.ctor());
          }

          public M1(callback: (arg0: UserClass, arg1: number, arg2: AnotherUserClass) => void): void {
            const callbackInstance = (arg0: ManagedObject, arg1: number, arg2: ManagedObject) => callback(ProxyBase.fromHandle(UserClass, arg0), arg1, ProxyBase.fromHandle(AnotherUserClass, arg2));
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, callbackInstance);
          }
        }

        """);
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithFuncUserClassReturnType()
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
            public Func<UserClass> M1() => () => (new UserClass() { Id = 1 });
        }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses[0], typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses[1], typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
        export class C1 extends ProxyBase {
          constructor() {
            super(TypeShimConfig.exports.N1.C1Interop.ctor());
          }

          public M1(): () => UserClass {
            const res = TypeShimConfig.exports.N1.C1Interop.M1(this.instance);
            return () => { const retVal = res(); return ProxyBase.fromHandle(UserClass, retVal) };
          }
        }
        
        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_MethodWithActionUserClassReturnType()
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
            public Action<UserClass> M1() => (UserClass u) => {};
        }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses[0], typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses[1], typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
        export class C1 extends ProxyBase {
          constructor() {
            super(TypeShimConfig.exports.N1.C1Interop.ctor());
          }

          public M1(): (arg0: UserClass) => void {
            const res = TypeShimConfig.exports.N1.C1Interop.M1(this.instance);
            return (arg0: UserClass | UserClass.Initializer) => res(arg0 instanceof UserClass ? arg0.instance : arg0);
          }
        }

        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_MethodWithPrimitiveDelegateReturnType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class C1
        {
            public Func<int> M1() => () => 1;
        }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses[0], typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
        export class C1 extends ProxyBase {
          constructor() {
            super(TypeShimConfig.exports.N1.C1Interop.ctor());
          }

          public M1(): () => number {
            return TypeShimConfig.exports.N1.C1Interop.M1(this.instance);
          }
        }

        """);
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithPrimitiveDelegatePropertyAndReturnType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class C1
        {
            public Func<bool, int> M1() => (bool b) => b ? 1 : 0;
        }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses[0], typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
        export class C1 extends ProxyBase {
          constructor() {
            super(TypeShimConfig.exports.N1.C1Interop.ctor());
          }

          public M1(): (arg0: boolean) => number {
            return TypeShimConfig.exports.N1.C1Interop.M1(this.instance);
          }
        }

        """);
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodWithPrimitiveParameterAndPrimitiveDelegateParameterAndReturnType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class C1
        {
            public Func<bool, int> M1(string s) => (bool b) => b ? 1 : 0;
        }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses[0], typeCache).Build();

        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.TypeScript);
        new TypescriptUserClassProxyRenderer(renderContext).Render();

        AssertEx.EqualOrDiff(renderContext.ToString(), """
        export class C1 extends ProxyBase {
          constructor() {
            super(TypeShimConfig.exports.N1.C1Interop.ctor());
          }

          public M1(s: string): (arg0: boolean) => number {
            return TypeShimConfig.exports.N1.C1Interop.M1(this.instance, s);
          }
        }

        """);
    }
}
