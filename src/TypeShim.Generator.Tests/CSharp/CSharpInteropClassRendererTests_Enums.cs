using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_Enums
{
    private static string RenderInteropClass(string members, string? underlyingType = null)
    {
        string enumDeclaration = underlyingType is null
            ? "public enum Color { Red, Green, Blue }"
            : $"public enum Color : {underlyingType} {{ Red, Green, Blue }}";

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            {{enum}}
            [TSExport]
            public class C1
            {
                private C1() {}
            {{members}}
            }
        """.Replace("{{enum}}", enumDeclaration).Replace("{{members}}", members));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedSymbols = [.. symbolExtractor.ExtractAllExportedSymbols()];
        INamedTypeSymbol classSymbol = exportedSymbols.First(s => s.Name == "C1");

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();
        return renderContext.ToString();
    }

    [Test]
    public void CSharpInteropClass_ScalarEnum_CastsBetweenIntAndEnum()
    {
        string interopClass = RenderInteropClass("""
                public static Color ScalarReturn() => Color.Red;
                public static void ScalarParam(Color c) {}
        """);

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int ScalarReturn()
    {
        return (int)global::N1.C1.ScalarReturn();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void ScalarParam([JSMarshalAs<JSType.Number>] int c)
    {
        global::N1.Color typed_c = (global::N1.Color)c;
        global::N1.C1.ScalarParam(typed_c);
    }
    public static global::N1.C1 FromObject(object obj)
    {
        return obj switch
        {
            global::N1.C1 instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
}

""");
    }

    [Test]
    public void CSharpInteropClass_ByteBackedEnum_CastsBetweenByteAndEnum()
    {
        string interopClass = RenderInteropClass("""
                public static Color ScalarReturn() => Color.Red;
                public static void ScalarParam(Color c) {}
        """, "byte");

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static byte ScalarReturn()
    {
        return (byte)global::N1.C1.ScalarReturn();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void ScalarParam([JSMarshalAs<JSType.Number>] byte c)
    {
        global::N1.Color typed_c = (global::N1.Color)c;
        global::N1.C1.ScalarParam(typed_c);
    }
    public static global::N1.C1 FromObject(object obj)
    {
        return obj switch
        {
            global::N1.C1 instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
}

""");
    }

    [Test]
    public void CSharpInteropClass_ShortBackedEnum_CastsBetweenShortAndEnum()
    {
        string interopClass = RenderInteropClass("""
                public static Color ScalarReturn() => Color.Red;
                public static void ScalarParam(Color c) {}
        """, "short");

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static short ScalarReturn()
    {
        return (short)global::N1.C1.ScalarReturn();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void ScalarParam([JSMarshalAs<JSType.Number>] short c)
    {
        global::N1.Color typed_c = (global::N1.Color)c;
        global::N1.C1.ScalarParam(typed_c);
    }
    public static global::N1.C1 FromObject(object obj)
    {
        return obj switch
        {
            global::N1.C1 instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
}

""");
    }

    [Test]
    public void CSharpInteropClass_IntBackedEnum_CastsBetweenIntAndEnum()
    {
        string interopClass = RenderInteropClass("""
                public static Color ScalarReturn() => Color.Red;
                public static void ScalarParam(Color c) {}
        """, "int");

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int ScalarReturn()
    {
        return (int)global::N1.C1.ScalarReturn();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void ScalarParam([JSMarshalAs<JSType.Number>] int c)
    {
        global::N1.Color typed_c = (global::N1.Color)c;
        global::N1.C1.ScalarParam(typed_c);
    }
    public static global::N1.C1 FromObject(object obj)
    {
        return obj switch
        {
            global::N1.C1 instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
}

""");
    }

    [Test]
    public void CSharpInteropClass_LongBackedEnumViaHelper_CastsBetweenLongAndEnum()
    {
        string interopClass = RenderInteropClass("""
                public static Color ScalarReturn() => Color.Red;
                public static void ScalarParam(Color c) {}
        """, "long");

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static long ScalarReturn()
    {
        return (long)global::N1.C1.ScalarReturn();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void ScalarParam([JSMarshalAs<JSType.Number>] long c)
    {
        global::N1.Color typed_c = (global::N1.Color)c;
        global::N1.C1.ScalarParam(typed_c);
    }
    public static global::N1.C1 FromObject(object obj)
    {
        return obj switch
        {
            global::N1.C1 instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
}

""");
    }

    [Test]
    public void CSharpInteropClass_LongBackedEnum_CastsBetweenLongAndEnum()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public enum Big : long { Zero = 0, Max = 9007199254740991 }
            [TSExport]
            public class C1
            {
                private C1() {}
                public Big Echo(Big b) => b;
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedSymbols = [.. symbolExtractor.ExtractAllExportedSymbols()];
        INamedTypeSymbol classSymbol = exportedSymbols.First(s => s.Name == "C1");
        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();
        string interopClass = renderContext.ToString();

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static long Echo([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] long b)
    {
        global::N1.C1 typed_instance = global::N1.C1Interop.FromObject(instance);
        global::N1.Big typed_b = (global::N1.Big)b;
        return (long)typed_instance.Echo(typed_b);
    }
    public static global::N1.C1 FromObject(object obj)
    {
        return obj switch
        {
            global::N1.C1 instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
}

""");
    }

    [Test]
    public void CSharpInteropClass_NullableEnum_CastsBetweenNullableIntAndNullableEnum()
    {
        string interopClass = RenderInteropClass("""
                public static Color? NullableReturn() => Color.Red;
                public static void NullableParam(Color? c) {}
        """);

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int? NullableReturn()
    {
        return (int?)global::N1.C1.NullableReturn();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void NullableParam([JSMarshalAs<JSType.Number>] int? c)
    {
        global::N1.Color? typed_c = c is { } cVal ? (global::N1.Color)cVal : null;
        global::N1.C1.NullableParam(typed_c);
    }
    public static global::N1.C1 FromObject(object obj)
    {
        return obj switch
        {
            global::N1.C1 instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
}

""");
    }

    [Test]
    public void CSharpInteropClass_EnumArray_ConvertsPerElementInBothDirections()
    {
        string interopClass = RenderInteropClass("""
                public static Color[] ArrayReturn() => [];
                public static void ArrayParam(Color[] c) {}
        """);

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static int[] ArrayReturn()
    {
        return Array.ConvertAll(global::N1.C1.ArrayReturn(), e => (int)e);
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void ArrayParam([JSMarshalAs<JSType.Array<JSType.Number>>] int[] c)
    {
        global::N1.Color[] typed_c = Array.ConvertAll(c, e => (global::N1.Color)e);
        global::N1.C1.ArrayParam(typed_c);
    }
    public static global::N1.C1 FromObject(object obj)
    {
        return obj switch
        {
            global::N1.C1 instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
}

""");
    }

    [Test]
    public void CSharpInteropClass_TaskEnum_CastsResultToInt()
    {
        string interopClass = RenderInteropClass("""
                public static Task<Color> TaskReturn() => Task.FromResult(Color.Red);
        """);

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static Task<int> TaskReturn()
    {
        return global::N1.C1.TaskReturn().ContinueWith(t => (int)t.Result, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
    }
    public static global::N1.C1 FromObject(object obj)
    {
        return obj switch
        {
            global::N1.C1 instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
}

""");
    }

    private static string RenderInteropClassWithInitializer(string members)
        => RenderInteropClassWithInitializer(members, underlyingType: null);

    private static string RenderInteropClassWithInitializer(string members, string? underlyingType)
    {
        string enumDeclaration = underlyingType is null
            ? "public enum Color { Red, Green, Blue }"
            : $"public enum Color : {underlyingType} {{ Red, Green, Blue }}";

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            {{enum}}
            [TSExport]
            public class C1
            {
            {{members}}
            }
        """.Replace("{{enum}}", enumDeclaration).Replace("{{members}}", members));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedSymbols = [.. symbolExtractor.ExtractAllExportedSymbols()];
        INamedTypeSymbol classSymbol = exportedSymbols.First(s => s.Name == "C1");

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();
        return renderContext.ToString();
    }

    [Test]
    public void CSharpInteropClass_EnumInitializerProperties_ParenthesizeScalarNullCoalesce()
    {
        // A public (implicit) constructor triggers the JSObject-initializer path. A non-nullable scalar enum
        // must be cast as (Color)(getter ?? throw ...) - the parentheses are required because casting to a
        // non-nullable value type would otherwise make "?? throw" invalid. Nullable/array enums use their
        // own conversion shapes.
        string interopClass = RenderInteropClassWithInitializer("""
                public Color Scalar { get; set; }
                public Color? Nullable { get; set; }
                public Color[] Arr { get; set; }
        """);

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.CompilerServices;
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
        if (initializer.HasProperty("Scalar"))
        {
            SetScalar(instance, (global::N1.Color)initializer.GetInt32Property("Scalar"));
        }
        if (initializer.HasProperty("Nullable"))
        {
            SetNullable(instance, initializer.GetInt32NullableProperty("Nullable") is { } NullableVal ? (global::N1.Color)NullableVal : null);
        }
        if (initializer.HasProperty("Arr"))
        {
            SetArr(instance, Array.ConvertAll(initializer.GetInt32ArrayProperty("Arr"), e => (global::N1.Color)e));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_Scalar([JSMarshalAs<JSType.Any>] object instance)
    {
        global::N1.C1 typed_instance = global::N1.C1Interop.FromObject(instance);
        return (int)typed_instance.Scalar;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_Scalar([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] int value)
    {
        global::N1.C1 typed_instance = global::N1.C1Interop.FromObject(instance);
        global::N1.Color typed_value = (global::N1.Color)value;
        typed_instance.Scalar = typed_value;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int? get_Nullable([JSMarshalAs<JSType.Any>] object instance)
    {
        global::N1.C1 typed_instance = global::N1.C1Interop.FromObject(instance);
        return (int?)typed_instance.Nullable;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_Nullable([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] int? value)
    {
        global::N1.C1 typed_instance = global::N1.C1Interop.FromObject(instance);
        global::N1.Color? typed_value = value is { } valueVal ? (global::N1.Color)valueVal : null;
        typed_instance.Nullable = typed_value;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static int[] get_Arr([JSMarshalAs<JSType.Any>] object instance)
    {
        global::N1.C1 typed_instance = global::N1.C1Interop.FromObject(instance);
        return Array.ConvertAll(typed_instance.Arr, e => (int)e);
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_Arr([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Number>>] int[] value)
    {
        global::N1.C1 typed_instance = global::N1.C1Interop.FromObject(instance);
        global::N1.Color[] typed_value = Array.ConvertAll(value, e => (global::N1.Color)e);
        typed_instance.Arr = typed_value;
    }
    public static global::N1.C1 FromObject(object obj)
    {
        return obj switch
        {
            global::N1.C1 instance => instance,
            JSObject jsObj => FromJSObject(jsObj),
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    public static global::N1.C1 FromJSObject(JSObject initializer)
    {
        using var _ = initializer;
        var instance = CreateInstance();
        if (initializer.HasProperty("Scalar"))
        {
            SetScalar(instance, (global::N1.Color)initializer.GetInt32Property("Scalar"));
        }
        if (initializer.HasProperty("Nullable"))
        {
            SetNullable(instance, initializer.GetInt32NullableProperty("Nullable") is { } NullableVal ? (global::N1.Color)NullableVal : null);
        }
        if (initializer.HasProperty("Arr"))
        {
            SetArr(instance, Array.ConvertAll(initializer.GetInt32ArrayProperty("Arr"), e => (global::N1.Color)e));
        }
        return instance;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern global::N1.C1 CreateInstance();

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Scalar")]
    private static extern void SetScalar(global::N1.C1 target, global::N1.Color value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Nullable")]
    private static extern void SetNullable(global::N1.C1 target, global::N1.Color? value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Arr")]
    private static extern void SetArr(global::N1.C1 target, global::N1.Color[] value);
}

""");
    }

    [TestCase(null, "int", "GetInt32Property")]
    [TestCase("byte", "byte", "GetByteProperty")]
    [TestCase("short", "short", "GetInt16Property")]
    [TestCase("int", "int", "GetInt32Property")]
    [TestCase("long", "long", "GetInt64Property")]
    public void CSharpInteropClass_EnumInitializerProperty_MarshalsViaUnderlyingTypeExtension(string? underlyingType, string interopType, string initializerMethod)
    {
        string interopClass = RenderInteropClassWithInitializer("""
                public Color Scalar { get; set; }
        """, underlyingType);

        AssertEx.EqualOrDiff(interopClass, """
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.CompilerServices;
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
        if (initializer.HasProperty("Scalar"))
        {
            SetScalar(instance, (global::N1.Color)initializer.{{initializerMethod}}("Scalar"));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static {{interopType}} get_Scalar([JSMarshalAs<JSType.Any>] object instance)
    {
        global::N1.C1 typed_instance = global::N1.C1Interop.FromObject(instance);
        return ({{interopType}})typed_instance.Scalar;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_Scalar([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] {{interopType}} value)
    {
        global::N1.C1 typed_instance = global::N1.C1Interop.FromObject(instance);
        global::N1.Color typed_value = (global::N1.Color)value;
        typed_instance.Scalar = typed_value;
    }
    public static global::N1.C1 FromObject(object obj)
    {
        return obj switch
        {
            global::N1.C1 instance => instance,
            JSObject jsObj => FromJSObject(jsObj),
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    public static global::N1.C1 FromJSObject(JSObject initializer)
    {
        using var _ = initializer;
        var instance = CreateInstance();
        if (initializer.HasProperty("Scalar"))
        {
            SetScalar(instance, (global::N1.Color)initializer.{{initializerMethod}}("Scalar"));
        }
        return instance;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern global::N1.C1 CreateInstance();

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Scalar")]
    private static extern void SetScalar(global::N1.C1 target, global::N1.Color value);
}

""".Replace("{{interopType}}", interopType).Replace("{{initializerMethod}}", initializerMethod));
    }
}
