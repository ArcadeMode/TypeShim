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
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, (arg0: ManagedObject) => callback(ProxyBase.fromHandle(UserClass, arg0)));
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
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, (arg0: ManagedObject) => callback(ProxyBase.fromHandle(UserClass, arg0)));
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
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, (arg0: ManagedObject, arg1: number) => callback(ProxyBase.fromHandle(UserClass, arg0), arg1));
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
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, (arg0: ManagedObject, arg1: number, arg2: ManagedObject) => callback(ProxyBase.fromHandle(UserClass, arg0), arg1, ProxyBase.fromHandle(AnotherUserClass, arg2)));
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
    public void TypeScriptUserClassProxy_MethodWithFuncUserClassParameterAndUserClassReturnType()
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
            public Func<UserClass, UserClass> M1() => (UserClass src) => (new UserClass() { Id = src.Id });
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

          public M1(): (arg0: UserClass | UserClass.Initializer) => UserClass {
            const res = TypeShimConfig.exports.N1.C1Interop.M1(this.instance);
            return (arg0: UserClass | UserClass.Initializer) => { const retVal = res(arg0 instanceof UserClass ? arg0.instance : arg0); return ProxyBase.fromHandle(UserClass, retVal) };
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

          public M1(): (arg0: UserClass | UserClass.Initializer) => void {
            const res = TypeShimConfig.exports.N1.C1Interop.M1(this.instance);
            return (arg0: UserClass | UserClass.Initializer) => res(arg0 instanceof UserClass ? arg0.instance : arg0);
          }
        }

        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_MethodWithActionUserClassParameterType()
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
            public void M1(Action<UserClass> action) => action(new UserClass() { Id = 1 });
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

          public M1(action: (arg0: UserClass) => void): void {
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, (arg0: ManagedObject) => action(ProxyBase.fromHandle(UserClass, arg0)));
          }
        }

        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_MethodWithFuncUserClassUserClassParameterType()
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
            public void M1(Func<UserClass, UserClass> func) {}
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

          public M1(func: (arg0: UserClass) => UserClass): void {
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, (arg0: ManagedObject) => { const retVal = func(ProxyBase.fromHandle(UserClass, arg0)); return retVal instanceof UserClass ? retVal.instance : retVal });
          }
        }
        
        """);
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodParameter_FuncUserClassNullableUserClass()
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
            public void M1(Func<UserClass, UserClass?> func) {}
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

          public M1(func: (arg0: UserClass) => UserClass | null): void {
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, (arg0: ManagedObject) => { const retVal = func(ProxyBase.fromHandle(UserClass, arg0)); return retVal ? retVal instanceof UserClass ? retVal.instance : retVal : null });
          }
        }
        
        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_MethodParameterType_NullableFuncUserClassNullableUserClass()
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
            public void M1(Func<UserClass, UserClass?>? func) {}
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

          public M1(func: ((arg0: UserClass) => UserClass | null) | null): void {
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, func ? (arg0: ManagedObject) => { const retVal = func(ProxyBase.fromHandle(UserClass, arg0)); return retVal ? retVal instanceof UserClass ? retVal.instance : retVal : null } : null);
          }
        }
        
        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_MethodReturnType_NullableFuncUserClassNullableUserClass()
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
            public Func<UserClass, UserClass?>? M1() {}
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

          public M1(): ((arg0: UserClass | UserClass.Initializer) => UserClass | null) | null {
            const res = TypeShimConfig.exports.N1.C1Interop.M1(this.instance);
            return res ? (arg0: UserClass | UserClass.Initializer) => { const retVal = res(arg0 instanceof UserClass ? arg0.instance : arg0); return retVal ? ProxyBase.fromHandle(UserClass, retVal) : null } : null;
          }
        }
        
        """);
    }

    [Test]
    public void TypeScriptUserClassProxy_MethodReturnType_NullableFuncUserClassUserClass()
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
            public Func<UserClass, UserClass>? M1() {}
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

          public M1(): ((arg0: UserClass | UserClass.Initializer) => UserClass) | null {
            const res = TypeShimConfig.exports.N1.C1Interop.M1(this.instance);
            return res ? (arg0: UserClass | UserClass.Initializer) => { const retVal = res(arg0 instanceof UserClass ? arg0.instance : arg0); return ProxyBase.fromHandle(UserClass, retVal) } : null;
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
    public void TypeScriptUserClassProxy_MethodReturnType_DelegateCharReturn_RendersConversion()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class C1
        {
            public Func<char> M1() {}
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

          public M1(): () => string {
            const res = TypeShimConfig.exports.N1.C1Interop.M1(this.instance);
            return () => String.fromCharCode(res());
          }
        }

        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_MethodReturnType_DelegateCharParameter_RendersConversion()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class C1
        {
            public Action<char> M1() {}
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

          public M1(): (arg0: string) => void {
            const res = TypeShimConfig.exports.N1.C1Interop.M1(this.instance);
            return (arg0: string) => res(arg0.charCodeAt(0));
          }
        }

        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_MethodParameterType_DelegateCharReturn_RendersConversion()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class C1
        {
            public void M1(Func<char> func) {}
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

          public M1(func: () => string): void {
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, () => func().charCodeAt(0));
          }
        }

        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_MethodParameterType_DelegateCharParameter_RendersConversion()
    {
        // TODO: Add tests for delegate+char in initializer
        // TODO: Add tests for delegate+char+proxy in initializer
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class C1
        {
            public void M1(Action<char> func) {}
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

          public M1(func: (arg0: string) => void): void {
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, (arg0: number) => func(String.fromCharCode(arg0)));
          }
        }
        
        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_MethodParameterType_DelegateCharAndUserClassParameter_RendersBothConversions()
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
            public void M1(Action<char, UserClass> func) {}
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

          public M1(func: (arg0: string, arg1: UserClass) => void): void {
            TypeShimConfig.exports.N1.C1Interop.M1(this.instance, (arg0: number, arg1: ManagedObject) => func(String.fromCharCode(arg0), ProxyBase.fromHandle(UserClass, arg1)));
          }
        }
        
        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_MethodReturnType_DelegateCharAndUserClassParameter_RendersBothConversions()
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
            public Action<char, UserClass> M1() {}
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

          public M1(): (arg0: string, arg1: UserClass | UserClass.Initializer) => void {
            const res = TypeShimConfig.exports.N1.C1Interop.M1(this.instance);
            return (arg0: string, arg1: UserClass | UserClass.Initializer) => res(arg0.charCodeAt(0), arg1 instanceof UserClass ? arg1.instance : arg1);
          }
        }
        
        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_PropertyType_ActionCharAndUserClassParameter_RendersBothConversions()
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
            public Action<char, UserClass> P1 { get; set; }
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
          constructor(jsObject: C1.Initializer) {
            super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: (arg0: number, arg1: ManagedObject) => jsObject.P1(String.fromCharCode(arg0), ProxyBase.fromHandle(UserClass, arg1)) }));
          }

          public get P1(): (arg0: string, arg1: UserClass | UserClass.Initializer) => void {
            const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
            return (arg0: string, arg1: UserClass | UserClass.Initializer) => res(arg0.charCodeAt(0), arg1 instanceof UserClass ? arg1.instance : arg1);
          }

          public set P1(value: (arg0: string, arg1: UserClass) => void) {
            TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, (arg0: number, arg1: ManagedObject) => value(String.fromCharCode(arg0), ProxyBase.fromHandle(UserClass, arg1)));
          }
        }
        
        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_PropertyType_FuncCharAndUserClassParameter_RendersBothConversions()
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
          constructor(jsObject: C1.Initializer) {
            super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: (arg0: number) => { const retVal = jsObject.P1(String.fromCharCode(arg0)); return retVal instanceof UserClass ? retVal.instance : retVal } }));
          }

          public get P1(): (arg0: string) => UserClass {
            const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
            return (arg0: string) => { const retVal = res(arg0.charCodeAt(0)); return ProxyBase.fromHandle(UserClass, retVal) };
          }

          public set P1(value: (arg0: string) => UserClass) {
            TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, (arg0: number) => { const retVal = value(String.fromCharCode(arg0)); return retVal instanceof UserClass ? retVal.instance : retVal });
          }
        }
        
        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_PropertyType_DelegateCharParameter_RendersConversions()
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
            public Action<char> P1 { get; set; }
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
          constructor(jsObject: C1.Initializer) {
            super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: (arg0: number) => jsObject.P1(String.fromCharCode(arg0)) }));
          }

          public get P1(): (arg0: string) => void {
            const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
            return (arg0: string) => res(arg0.charCodeAt(0));
          }

          public set P1(value: (arg0: string) => void) {
            TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, (arg0: number) => value(String.fromCharCode(arg0)));
          }
        }
        
        """);
    }
    
    [Test]
    public void TypeScriptUserClassProxy_PropertyType_DelegateUserClassParameter_RendersConversions()
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
            public Action<UserClass> P1 { get; set; }
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
          constructor(jsObject: C1.Initializer) {
            super(TypeShimConfig.exports.N1.C1Interop.ctor({ ...jsObject, P1: (arg0: ManagedObject) => jsObject.P1(ProxyBase.fromHandle(UserClass, arg0)) }));
          }

          public get P1(): (arg0: UserClass | UserClass.Initializer) => void {
            const res = TypeShimConfig.exports.N1.C1Interop.get_P1(this.instance);
            return (arg0: UserClass | UserClass.Initializer) => res(arg0 instanceof UserClass ? arg0.instance : arg0);
          }

          public set P1(value: (arg0: UserClass) => void) {
            TypeShimConfig.exports.N1.C1Interop.set_P1(this.instance, (arg0: ManagedObject) => value(ProxyBase.fromHandle(UserClass, arg0)));
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
