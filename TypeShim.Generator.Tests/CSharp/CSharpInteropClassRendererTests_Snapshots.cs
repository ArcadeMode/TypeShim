using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_Snapshots
{
    [TestCase("string", "string", "JSType.String", "GetPropertyAsString")]
    [TestCase("double", "double", "JSType.Number", "GetPropertyAsDoubleNullable")]
    [TestCase("bool", "bool", "JSType.Boolean", "GetPropertyAsBooleanNullable")]
    public void CSharpInteropClass_SupportedPropertyType_GeneratesFromJSObjectMethod(string typeExpression, string interopTypeExpression, string jsType, string jsObjectMethod)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public {{typeExpression}} P1 { get; set; }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
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
            P1 = jsObject.{{jsObjectMethod}}("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
    [JSExport]
    [return: JSMarshalAs<{{jsType}}>]
    public static {{typeExpression}} get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<{{jsType}}>] {{typeExpression}} value)
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
            P1 = jsObject.{{jsObjectMethod}}("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
}

""".Replace("{{typeExpression}}", interopTypeExpression)
   .Replace("{{jsType}}", jsType)
   .Replace("{{jsObjectMethod}}", jsObjectMethod));
    }

    [TestCase("string", "string", "JSType.String")]
    [TestCase("double", "double", "JSType.Number")]
    [TestCase("bool", "bool", "JSType.Boolean")]
    public void CSharpInteropClass_SupportedPropertyType_AndNotExportedReferenceType_GeneratesNoFromJSObjectMethod(string typeExpression, string interopTypeExpression, string jsType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            using System.Collections.Generic;
            namespace N1;
            [TSExport]
            public class C1
            {
                public List<{{typeExpression}}> P2 { get; set; }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
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
            P2 = (List<{{typeExpression}}>)jsObject.GetPropertyAsObject("P2") ?? throw new ArgumentException("Non-nullable property 'P2' missing or of invalid type", nameof(jsObject)),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object get_P2([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return (object)typed_instance.P2;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P2([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object value)
    {
        C1 typed_instance = (C1)instance;
        List<{{typeExpression}}> typed_value = (List<{{typeExpression}}>)value;
        typed_instance.P2 = typed_value;
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
            P2 = (List<{{typeExpression}}>)jsObject.GetPropertyAsObject("P2") ?? throw new ArgumentException("Non-nullable property 'P2' missing or of invalid type", nameof(jsObject)),
        };
    }
}

""".Replace("{{typeExpression}}", interopTypeExpression)
   .Replace("{{jsType}}", jsType));
    }

    [TestCase("Version")]
    [TestCase("Uri")]
    public void CSharpInteropClass_SupportedPropertyType_AndNotExportedArrayType_GeneratesNoFromJSObjectMethod(string typeName)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public {{typeName}}[] P1 { get; set; }
                public int P2 { get; set; }
            }
        """.Replace("{{typeName}}", typeName));
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.Last();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        AssertEx.EqualOrDiff(interopClass, """    
#nullable enable
// TypeShim generated TypeScript interop definitions
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
            P1 = ({{typeName}}[])jsObject.GetPropertyAsObjectArray("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
            P2 = jsObject.GetPropertyAsInt32Nullable("P2") ?? throw new ArgumentException("Non-nullable property 'P2' missing or of invalid type", nameof(jsObject)),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Any>>]
    public static object[] get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return (object[])typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object[] value)
    {
        C1 typed_instance = (C1)instance;
        {{typeName}}[] typed_value = ({{typeName}}[])value;
        typed_instance.P1 = typed_value;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P2([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P2;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P2([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] int value)
    {
        C1 typed_instance = (C1)instance;
        typed_instance.P2 = value;
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
            P1 = ({{typeName}}[])jsObject.GetPropertyAsObjectArray("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
            P2 = jsObject.GetPropertyAsInt32Nullable("P2") ?? throw new ArgumentException("Non-nullable property 'P2' missing or of invalid type", nameof(jsObject)),
        };
    }
}

""".Replace("{{typeName}}", typeName));
    }

    [TestCase("Version")]
    [TestCase("Uri")]
    public void CSharpInteropClass_SupportedPropertyType_AndNotExportedTaskType_GeneratesNoFromJSObjectMethod(string typeName)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public Task<{{typeName}}> P1 { get; set; }
                public int P2 { get; set; }
            }
        """.Replace("{{typeName}}", typeName));
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.Last();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
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
        TaskCompletionSource<{{typeName}}> P1Tcs = new();
        (jsObject.GetPropertyAsObjectTask("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject))).ContinueWith(t => {
            if (t.IsFaulted) P1Tcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) P1Tcs.SetCanceled();
            else P1Tcs.SetResult(({{typeName}})t.Result);
        }, TaskContinuationOptions.ExecuteSynchronously);
        return new C1()
        {
            P1 = P1Tcs.Task,
            P2 = jsObject.GetPropertyAsInt32Nullable("P2") ?? throw new ArgumentException("Non-nullable property 'P2' missing or of invalid type", nameof(jsObject)),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
    public static Task<object> get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        TaskCompletionSource<object> retValTcs = new();
        (typed_instance.P1).ContinueWith(t => {
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
        TaskCompletionSource<{{typeName}}> valueTcs = new();
        (value).ContinueWith(t => {
            if (t.IsFaulted) valueTcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) valueTcs.SetCanceled();
            else valueTcs.SetResult(({{typeName}})t.Result);
        }, TaskContinuationOptions.ExecuteSynchronously);
        Task<{{typeName}}> typed_value = valueTcs.Task;
        typed_instance.P1 = typed_value;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P2([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P2;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P2([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] int value)
    {
        C1 typed_instance = (C1)instance;
        typed_instance.P2 = value;
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
        TaskCompletionSource<{{typeName}}> P1Tcs = new();
        (jsObject.GetPropertyAsObjectTask("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject))).ContinueWith(t => {
            if (t.IsFaulted) P1Tcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) P1Tcs.SetCanceled();
            else P1Tcs.SetResult(({{typeName}})t.Result);
        }, TaskContinuationOptions.ExecuteSynchronously);
        return new C1()
        {
            P1 = P1Tcs.Task,
            P2 = jsObject.GetPropertyAsInt32Nullable("P2") ?? throw new ArgumentException("Non-nullable property 'P2' missing or of invalid type", nameof(jsObject)),
        };
    }
}

""".Replace("{{typeName}}", typeName));
    }
}
