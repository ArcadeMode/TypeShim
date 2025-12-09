using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_SystemTaskReturnType
{
    [TestCase("Byte", "byte")]
    [TestCase("byte", "byte")]
    [TestCase("Int16", "short")]
    [TestCase("short", "short")]
    [TestCase("Int32", "int")]
    [TestCase("int", "int")]
    [TestCase("Int64", "long")]
    [TestCase("long", "long")]
    [TestCase("Single", "float")]
    [TestCase("float", "float")]
    [TestCase("Double", "double")]
    [TestCase("double", "double")]
    [TestCase("IntPtr", "nint")]
    [TestCase("nint", "nint")]
    public void CSharpInteropClass_StaticMethod_HasJSTypePromiseNumber_ForNumericReturnType(string typeExpression, string interopTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class C1
            {
                public static Task<{{typeExpression}}> M1()
                {
                    return Task.FromResult(1);
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        string interopClass = new CSharpInteropClassRenderer(classInfo).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static async Task<{{typeExpression}}> M1()
    {
        return await C1.M1();
    }
}

""".Replace("{{typeExpression}}", interopTypeExpression)));
    }

    [Test]
    public void CSharpInteropClass_StaticMethod_HasJSTypePromiseAny_ForUserClassReturnType()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class MyClass
            {
                public void M1()
                {
                }
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class C1
            {
                public static Task<MyClass> M1()
                {
                    return Task.FromResult(new MyClass());
                }
            }
        """);

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([userClass, syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.Last();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        string interopClass = new CSharpInteropClassRenderer(classInfo).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
    public static async Task<object> M1()
    {
        return await C1.M1();
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_DynamicMethod_HasJSTypePromiseAny_ForUserClassReturnType()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class MyClass
            {
                public void M1()
                {
                }
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class C1
            {
                public Task<MyClass> M1()
                {
                    return Task.FromResult(new MyClass());
                }
            }
        """);

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([userClass, syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.Last();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        string interopClass = new CSharpInteropClassRenderer(classInfo).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
    public static async Task<object> M1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return await typed_instance.M1();
    }
}

"""));
    }

    [TestCase("Version", "new Version(1,2,3,4)")]
    [TestCase("Uri", "new Uri(\"http://example.com\")")]
    public void CSharpInteropClass_StaticMethod_HasJSTypePromiseAny_ForNonUserClassReturnType(string typeName, string objectCreation)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class C1
            {
                public static Task<{{typeName}}> M1()
                {
                    return Task.FromResult({objectCreation});
                }
            }
        """.Replace("{{typeName}}", typeName).Replace("{{objectCreation}}", objectCreation));
        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.Last();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        string interopClass = new CSharpInteropClassRenderer(classInfo).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
    public static async Task<object> M1()
    {
        return await C1.M1();
    }
}

"""));
    }
}
