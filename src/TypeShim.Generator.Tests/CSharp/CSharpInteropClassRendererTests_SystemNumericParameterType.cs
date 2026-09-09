using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_SystemNumericParameterType
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
    public void CSharpInteropClass_StaticMethod_RenderedWithJSTypeNumber_ForSupportedNumericParameterType(string typeExpression, string interopTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public static class C1
            {
                public static void M1({{typeExpression}} arg1)
                {
                    var x = arg1;
                }
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

        Assert.That(interopClass, Is.EqualTo("""    
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Number>] {{typeExpression}} arg1)
    {
        C1.M1(arg1);
    }
}

""".Replace("{{typeExpression}}", interopTypeExpression)));
    }

    [TestCase("Byte?", "byte?")]
    [TestCase("byte?", "byte?")]
    [TestCase("Int16?", "short?")]
    [TestCase("short?", "short?")]
    [TestCase("Int32?", "int?")]
    [TestCase("int?", "int?")]
    [TestCase("Int64?", "long?")]
    [TestCase("long?", "long?")]
    [TestCase("Single?", "float?")]
    [TestCase("float?", "float?")]
    [TestCase("Double?", "double?")]
    [TestCase("double?", "double?")]
    [TestCase("IntPtr?", "nint?")]
    [TestCase("nint?", "nint?")]
    public void CSharpInteropClass_StaticMethod_RenderedWithJSTypeNumber_ForSupportedNullableNumericParameterType(string typeExpression, string interopTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public static class C1
            {
                public static void M1({{typeExpression}} arg1)
                {
                    var x = arg1;
                }
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

        Assert.That(interopClass, Is.EqualTo("""    
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Number>] {{typeExpression}} arg1)
    {
        C1.M1(arg1);
    }
}

""".Replace("{{typeExpression}}", interopTypeExpression)));
    }

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
    public void CSharpInteropClass_InstanceMethod_RenderedWithJSTypeNumber_ForSupportedNumericParameterType(string typeExpression, string interopTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                private C1() {}
                public void M1({{typeExpression}} arg1)
                {
                    var x = arg1;
                }
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

        Assert.That(interopClass, Is.EqualTo("""    
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] {{typeExpression}} arg1)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        typed_instance.M1(arg1);
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

""".Replace("{{typeExpression}}", interopTypeExpression)));
    }

    [TestCase("Byte", "byte", "GetPropertyAsByte")]
    [TestCase("byte", "byte", "GetPropertyAsByte")]
    [TestCase("Int16", "short", "GetPropertyAsInt16")]
    [TestCase("short", "short", "GetPropertyAsInt16")]
    [TestCase("Int32", "int", "GetPropertyAsInt32")]
    [TestCase("int", "int", "GetPropertyAsInt32")]
    [TestCase("Int64", "long", "GetPropertyAsInt64")]
    [TestCase("long", "long", "GetPropertyAsInt64")]
    [TestCase("Single", "float", "GetPropertyAsSingle")]
    [TestCase("float", "float", "GetPropertyAsSingle")]
    [TestCase("Double", "double", "GetPropertyAsDouble")]
    [TestCase("double", "double", "GetPropertyAsDouble")]
    [TestCase("IntPtr", "nint", "GetPropertyAsIntPtr")]
    [TestCase("nint", "nint", "GetPropertyAsIntPtr")]
    public void CSharpInteropClass_InstanceProperty_WithSupportedNumericParameterType(string typeExpression, string interopTypeExpression, string initializerMethod)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
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
        var instance = CreateInstance();
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, initializer.{{initializerMethod}}("P1")!);
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static {{typeExpression}} get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] {{typeExpression}} value)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
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
        var instance = CreateInstance();
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, initializer.{{initializerMethod}}("P1")!);
        }
        return instance;
    }
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, {{typeExpression}} value);
}

""".Replace("{{typeExpression}}", interopTypeExpression).Replace("{{initializerMethod}}", initializerMethod));
    }
}
