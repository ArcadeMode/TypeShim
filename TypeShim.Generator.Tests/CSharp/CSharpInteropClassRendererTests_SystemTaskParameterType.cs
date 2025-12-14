using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework.Internal;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_SystemTaskParameterType
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
    public void CSharpInteropClass_StaticMethod_HasJSTypePromiseNumber_ForNumericTaskParameterType(string typeExpression, string interopTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public static void M1(Task<{{typeExpression}}> task)
                {
                    bool b = task.IsCompleted;
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
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
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Promise<JSType.Number>>] Task<{{typeExpression}}> task)
    {
        C1.M1(task);
    }
}

""".Replace("{{typeExpression}}", interopTypeExpression)));
    }

    [Test]
    public void CSharpInteropClass_StaticMethod_HasJSTypePromiseAny_ForUserClassTaskParameterType()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
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
            [TSExport]
            public class C1
            {
                public static void M1(Task<MyClass> task)
                {
                    bool b = task.IsCompleted;
                }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        string interopClass = new CSharpInteropClassRenderer(classInfo).Render();

        // Important assertion here, Task<object> required for interop, cannot be simply casted to Task<MyClass>
        // the return type is void so we cannot await either, hence the TaskCompletionSource-based conversion.
        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Promise<JSType.Any>>] Task<object> task)
    {
        TaskCompletionSource<MyClass> taskTcs = new();
        task.ContinueWith(t =>
            t.IsFaulted ? taskTcs.SetException(t.Exception.InnerExceptions)
            : t.IsCanceled ? taskTcs.SetCanceled()
            : taskTcs.SetResult((MyClass)t.Result), TaskContinuationOptions.ExecuteSynchronously);
        Task<MyClass> typed_task = taskTcs.Task;
        C1.M1(typed_task);
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_DynamicMethod_HasJSTypePromiseAny_ForUserClassTaskParameterType()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
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
            [TSExport]
            public class C1
            {
                public static void M1(Task<MyClass> task)
                {
                    var x = new MyClass();
                }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

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
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Promise<JSType.Any>>] Task<object> task)
    {
        TaskCompletionSource<MyClass> taskTcs = new();
        task.ContinueWith(t =>
            t.IsFaulted ? taskTcs.SetException(t.Exception.InnerExceptions)
            : t.IsCanceled ? taskTcs.SetCanceled()
            : taskTcs.SetResult((MyClass)t.Result), TaskContinuationOptions.ExecuteSynchronously);
        Task<MyClass> typed_task = taskTcs.Task;
        C1.M1(typed_task);
    }
}

"""));
    }

    [TestCase("Version", "new Version(1,2,3,4)")]
    [TestCase("Uri", "new Uri(\"http://example.com\")")]
    public void CSharpInteropClass_StaticMethod_HasJSTypePromiseAny_ForNonUserClassTaskParameterType(string typeName, string objectCreation)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public static void M1(Task<{{typeName}}> task)
                {
                    var x = {{objectCreation}};
                }
            }
        """.Replace("{{typeName}}", typeName).Replace("{{objectCreation}}", objectCreation));
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
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
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Promise<JSType.Any>>] Task<object> task)
    {
        TaskCompletionSource<{{typeName}}> taskTcs = new();
        task.ContinueWith(t =>
            t.IsFaulted ? taskTcs.SetException(t.Exception.InnerExceptions)
            : t.IsCanceled ? taskTcs.SetCanceled()
            : taskTcs.SetResult(({{typeName}})t.Result), TaskContinuationOptions.ExecuteSynchronously);
        Task<{{typeName}}> typed_task = taskTcs.Task;
        C1.M1(typed_task);
    }
}

""".Replace("{{typeName}}", typeName)));
    }
}
