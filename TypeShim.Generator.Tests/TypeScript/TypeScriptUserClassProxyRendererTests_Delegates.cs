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
            TypeShimConfig.exports.TypeShim.Sample.DelegatesTestInterop.InvokeExportedClassAction(this.instance, callbackInstance);
          }
        }

        """);
    }
}
