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
    [TestCase("string", "string", "JSType.String", "GetPropertyAsStringNullable")]
    [TestCase("double", "double", "JSType.Number", "GetPropertyAsDoubleNullable")]
    [TestCase("bool", "bool", "JSType.Boolean", "GetPropertyAsBooleanNullable")]
    public void CSharpInteropClass_SupportedPropertyType_GeneratesFromJSObjectMethod(string typeExpression, string interopTypeExpression, string jsType, string initializerMethod)
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

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
    public static object ctor([JSMarshalAs<JSType.Object>] JSObject initializer)
    {
        using var _ = initializer;
        return new C1()
        {
            P1 = initializer.{{initializerMethod}}("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer)),
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
    public static C1 FromJSObject(JSObject initializer)
    {
        using var _ = initializer;
        return new C1()
        {
            P1 = initializer.{{initializerMethod}}("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer)),
        };
    }
}

""".Replace("{{typeExpression}}", interopTypeExpression)
   .Replace("{{jsType}}", jsType)
   .Replace("{{initializerMethod}}", initializerMethod));
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.Last();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
    public static object ctor([JSMarshalAs<JSType.Object>] JSObject initializer)
    {
        using var _ = initializer;
        return new C1()
        {
            P1 = ({{typeName}}[])initializer.GetPropertyAsObjectArrayNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer)),
            P2 = initializer.GetPropertyAsInt32Nullable("P2") ?? throw new ArgumentException("Non-nullable property 'P2' missing or of invalid type", nameof(initializer)),
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
    public static C1 FromJSObject(JSObject initializer)
    {
        using var _ = initializer;
        return new C1()
        {
            P1 = ({{typeName}}[])initializer.GetPropertyAsObjectArrayNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer)),
            P2 = initializer.GetPropertyAsInt32Nullable("P2") ?? throw new ArgumentException("Non-nullable property 'P2' missing or of invalid type", nameof(initializer)),
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.Last();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
    public static object ctor([JSMarshalAs<JSType.Object>] JSObject initializer)
    {
        using var _ = initializer;
        return new C1()
        {
            P1 = (initializer.GetPropertyAsObjectTaskNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer))).ContinueWith(t => ({{typeName}})t.Result, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously),
            P2 = initializer.GetPropertyAsInt32Nullable("P2") ?? throw new ArgumentException("Non-nullable property 'P2' missing or of invalid type", nameof(initializer)),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
    public static Task<object> get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1.ContinueWith(t => (object)t.Result, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Promise<JSType.Any>>] Task<object> value)
    {
        C1 typed_instance = (C1)instance;
        Task<{{typeName}}> typed_value = value.ContinueWith(t => ({{typeName}})t.Result, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
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
    public static C1 FromJSObject(JSObject initializer)
    {
        using var _ = initializer;
        return new C1()
        {
            P1 = (initializer.GetPropertyAsObjectTaskNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer))).ContinueWith(t => ({{typeName}})t.Result, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously),
            P2 = initializer.GetPropertyAsInt32Nullable("P2") ?? throw new ArgumentException("Non-nullable property 'P2' missing or of invalid type", nameof(initializer)),
        };
    }
}

""".Replace("{{typeName}}", typeName));
    }
}
