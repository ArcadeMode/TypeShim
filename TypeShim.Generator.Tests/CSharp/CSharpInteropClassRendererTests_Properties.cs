using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_Properties
{
    [Test]
    public void CSharpInteropClass_InstanceProperty_WithUserClassType()
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
                public MyClass P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last()).Build();
        RenderContext renderContext = new([classInfo, userClassInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object value)
    {
        C1 typed_instance = (C1)instance;
        MyClass typed_value = MyClassInterop.FromObject(value);
        typed_instance.P1 = typed_value;
    }
    public static C1 FromObject(object obj)
    {
        return obj switch
        {
            C1 instance => instance,
            JSObject jsObj => FromJSObject(jsObj),
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    public static C1 FromJSObject(JSObject jsObject)
    {
        return new()
        {
            P1 = MyClassInterop.FromObject(jsObject.GetPropertyAsJSObject("P1")),
        };
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithNullableUserClassType()
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
                public MyClass? P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last()).Build();
        RenderContext renderContext = new([classInfo, userClassInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object? get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object? value)
    {
        C1 typed_instance = (C1)instance;
        MyClass? typed_value = value != null ? MyClassInterop.FromObject(value) : null;
        typed_instance.P1 = typed_value;
    }
    public static C1 FromObject(object obj)
    {
        return obj switch
        {
            C1 instance => instance,
            JSObject jsObj => FromJSObject(jsObj),
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    public static C1 FromJSObject(JSObject jsObject)
    {
        var P1Tmp = jsObject.GetPropertyAsJSObject("P1");
        return new()
        {
            P1 = P1Tmp != null ? MyClassInterop.FromObject(P1Tmp) : null,
        };
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithSystemObjectType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public object P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        RenderContext renderContext = new([classInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object value)
    {
        C1 typed_instance = (C1)instance;
        typed_instance.P1 = value;
    }
    public static C1 FromObject(object obj)
    {
        return obj switch
        {
            C1 instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithIntArrayType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public int[] P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        RenderContext renderContext = new([classInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
    {
        return new C1()
        {
            P1 = jsObject.GetPropertyAsInt32Array("P1"),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static int[] get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Number>>] int[] value)
    {
        C1 typed_instance = (C1)instance;
        typed_instance.P1 = value;
    }
    public static C1 FromObject(object obj)
    {
        return obj switch
        {
            C1 instance => instance,
            JSObject jsObj => FromJSObject(jsObj),
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    public static C1 FromJSObject(JSObject jsObject)
    {
        return new C1()
        {
            P1 = jsObject.GetPropertyAsInt32Array("P1"),
        };
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithUserClassArrayType()
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
                public MyClass[] P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last()).Build();
        RenderContext renderContext = new([classInfo, userClassInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Any>>]
    public static object[] get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object[] value)
    {
        C1 typed_instance = (C1)instance;
        MyClass[] typed_value = Array.ConvertAll(value, e => MyClassInterop.FromObject(e));
        typed_instance.P1 = typed_value;
    }
    public static C1 FromObject(object obj)
    {
        return obj switch
        {
            C1 instance => instance,
            JSObject jsObj => FromJSObject(jsObj),
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    public static C1 FromJSObject(JSObject jsObject)
    {
        return new()
        {
            P1 = Array.ConvertAll(jsObject.GetPropertyAsJSObjectArray("P1"), e => MyClassInterop.FromObject(e)),
        };
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithNullableUserClassArrayType()
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
                public MyClass?[] P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last()).Build();
        RenderContext renderContext = new([classInfo, userClassInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Any>>]
    public static object?[] get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object?[] value)
    {
        C1 typed_instance = (C1)instance;
        MyClass?[] typed_value = Array.ConvertAll(value, e => e != null ? MyClassInterop.FromObject(e) : null);
        typed_instance.P1 = typed_value;
    }
    public static C1 FromObject(object obj)
    {
        return obj switch
        {
            C1 instance => instance,
            JSObject jsObj => FromJSObject(jsObj),
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    public static C1 FromJSObject(JSObject jsObject)
    {
        return new()
        {
            P1 = Array.ConvertAll(jsObject.GetPropertyAsJSObjectArray("P1"), e => e != null ? MyClassInterop.FromObject(e) : null),
        };
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithUserClassNullableArrayType()
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
                public MyClass[]? P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last()).Build();
        RenderContext renderContext = new([classInfo, userClassInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Any>>]
    public static object[]? get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object[]? value)
    {
        C1 typed_instance = (C1)instance;
        MyClass[]? typed_value = value != null ? Array.ConvertAll(value, e => MyClassInterop.FromObject(e)) : null;
        typed_instance.P1 = typed_value;
    }
    public static C1 FromObject(object obj)
    {
        return obj switch
        {
            C1 instance => instance,
            JSObject jsObj => FromJSObject(jsObj),
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    public static C1 FromJSObject(JSObject jsObject)
    {
        var P1Tmp = jsObject.GetPropertyAsJSObjectArray("P1");
        return new()
        {
            P1 = P1Tmp != null ? Array.ConvertAll(P1Tmp, e => MyClassInterop.FromObject(e)) : null,
        };
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithNullableUserClassNullableArrayType()
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
                public MyClass?[]? P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last()).Build();
        RenderContext renderContext = new([classInfo, userClassInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Any>>]
    public static object?[]? get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object?[]? value)
    {
        C1 typed_instance = (C1)instance;
        MyClass?[]? typed_value = value != null ? Array.ConvertAll(value, e => e != null ? MyClassInterop.FromObject(e) : null) : null;
        typed_instance.P1 = typed_value;
    }
    public static C1 FromObject(object obj)
    {
        return obj switch
        {
            C1 instance => instance,
            JSObject jsObj => FromJSObject(jsObj),
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    public static C1 FromJSObject(JSObject jsObject)
    {
        var P1Tmp = jsObject.GetPropertyAsJSObjectArray("P1");
        return new()
        {
            P1 = P1Tmp != null ? Array.ConvertAll(P1Tmp, e => e != null ? MyClassInterop.FromObject(e) : null) : null,
        };
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithUserClassTaskType()
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
                public Task<MyClass> P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last()).Build();
        RenderContext renderContext = new([classInfo, userClassInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
    public static Task<object> get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        TaskCompletionSource<object> retValTcs = new();
        typed_instance.P1.ContinueWith(t => {
            if (t.IsFaulted) retValTcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) retValTcs.SetCanceled();
            else retValTcs.SetResult((object)t.Result);
        }, TaskContinuationOptions.ExecuteSynchronously);
        return retValTcs.Task;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Promise<JSType.Any>>] Task<object> value)
    {
        C1 typed_instance = (C1)instance;
        TaskCompletionSource<MyClass> valueTcs = new();
        value.ContinueWith(t => {
            if (t.IsFaulted) valueTcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) valueTcs.SetCanceled();
            else valueTcs.SetResult(MyClassInterop.FromObject(t.Result));
        }, TaskContinuationOptions.ExecuteSynchronously);
        Task<MyClass> typed_value = valueTcs.Task;
        typed_instance.P1 = typed_value;
    }
    public static C1 FromObject(object obj)
    {
        return obj switch
        {
            C1 instance => instance,
            JSObject jsObj => FromJSObject(jsObj),
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    public static C1 FromJSObject(JSObject jsObject)
    {
        var P1Tmp = jsObject.GetPropertyAsJSObjectTask("P1");
        TaskCompletionSource<MyClass> P1Tcs = new();
        P1Tmp.ContinueWith(t => {
            if (t.IsFaulted) P1Tcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) P1Tcs.SetCanceled();
            else P1Tcs.SetResult(MyClassInterop.FromObject(t.Result));
        }, TaskContinuationOptions.ExecuteSynchronously);
        return new()
        {
            P1 = P1Tcs.Task,
        };
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithNullableUserClassTaskType()
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
                public Task<MyClass?> P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last()).Build();
        RenderContext renderContext = new([classInfo, userClassInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
    public static Task<object?> get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        TaskCompletionSource<object?> retValTcs = new();
        typed_instance.P1.ContinueWith(t => {
            if (t.IsFaulted) retValTcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) retValTcs.SetCanceled();
            else retValTcs.SetResult(t.Result != null ? (object)t.Result : null);
        }, TaskContinuationOptions.ExecuteSynchronously);
        return retValTcs.Task;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Promise<JSType.Any>>] Task<object?> value)
    {
        C1 typed_instance = (C1)instance;
        TaskCompletionSource<MyClass?> valueTcs = new();
        value.ContinueWith(t => {
            if (t.IsFaulted) valueTcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) valueTcs.SetCanceled();
            else valueTcs.SetResult(t.Result != null ? MyClassInterop.FromObject(t.Result) : null);
        }, TaskContinuationOptions.ExecuteSynchronously);
        Task<MyClass?> typed_value = valueTcs.Task;
        typed_instance.P1 = typed_value;
    }
    public static C1 FromObject(object obj)
    {
        return obj switch
        {
            C1 instance => instance,
            JSObject jsObj => FromJSObject(jsObj),
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    public static C1 FromJSObject(JSObject jsObject)
    {
        var P1Tmp = jsObject.GetPropertyAsJSObjectTask("P1");
        TaskCompletionSource<MyClass?> P1Tcs = new();
        P1Tmp.ContinueWith(t => {
            if (t.IsFaulted) P1Tcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) P1Tcs.SetCanceled();
            else P1Tcs.SetResult(t.Result != null ? MyClassInterop.FromObject(t.Result) : null);
        }, TaskContinuationOptions.ExecuteSynchronously);
        return new()
        {
            P1 = P1Tcs.Task,
        };
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithNullableUserClassNullableTaskType()
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
                public Task<MyClass?>? P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last()).Build();
        RenderContext renderContext = new([classInfo, userClassInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
    public static Task<object?>? get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        TaskCompletionSource<object?>? retValTcs = typed_instance.P1 != null ? new() : null;
        typed_instance.P1?.ContinueWith(t => {
            if (t.IsFaulted) retValTcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) retValTcs.SetCanceled();
            else retValTcs.SetResult(t.Result != null ? (object)t.Result : null);
        }, TaskContinuationOptions.ExecuteSynchronously);
        return retValTcs?.Task;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Promise<JSType.Any>>] Task<object?>? value)
    {
        C1 typed_instance = (C1)instance;
        TaskCompletionSource<MyClass?>? valueTcs = value != null ? new() : null;
        value?.ContinueWith(t => {
            if (t.IsFaulted) valueTcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) valueTcs.SetCanceled();
            else valueTcs.SetResult(t.Result != null ? MyClassInterop.FromObject(t.Result) : null);
        }, TaskContinuationOptions.ExecuteSynchronously);
        Task<MyClass?>? typed_value = valueTcs?.Task;
        typed_instance.P1 = typed_value;
    }
    public static C1 FromObject(object obj)
    {
        return obj switch
        {
            C1 instance => instance,
            JSObject jsObj => FromJSObject(jsObj),
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    public static C1 FromJSObject(JSObject jsObject)
    {
        var P1Tmp = jsObject.GetPropertyAsJSObjectTask("P1");
        TaskCompletionSource<MyClass?>? P1Tcs = P1Tmp != null ? new() : null;
        P1Tmp?.ContinueWith(t => {
            if (t.IsFaulted) P1Tcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) P1Tcs.SetCanceled();
            else P1Tcs.SetResult(t.Result != null ? MyClassInterop.FromObject(t.Result) : null);
        }, TaskContinuationOptions.ExecuteSynchronously);
        return new()
        {
            P1 = P1Tcs?.Task,
        };
    }
}

"""));
    }

    [TestCase("Version")]
    [TestCase("Uri")]
    public void CSharpInteropClass_InstanceProperty_WithNonUserClassArrayType_HasNoFromJSObjectMethod(string typeName) //i.e. is not snapshot compatible
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public {{typeName}}[] P1 { get; set; }
            }
        """.Replace("{{typeName}}", typeName));
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.Last();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        RenderContext renderContext = new([classInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Any>>]
    public static object[] get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object[] value)
    {
        C1 typed_instance = (C1)instance;
        {{typeName}}[] typed_value = ({{typeName}}[])value;
        typed_instance.P1 = typed_value;
    }
    public static C1 FromObject(object obj)
    {
        return obj switch
        {
            C1 instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
}

""".Replace("{{typeName}}", typeName)));
    }
}
