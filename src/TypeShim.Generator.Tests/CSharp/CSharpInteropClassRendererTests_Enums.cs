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
        return new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();
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
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int ScalarReturn()
    {
        return (int)C1.ScalarReturn();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void ScalarParam([JSMarshalAs<JSType.Number>] int c)
    {
        Color typed_c = (Color)c;
        C1.ScalarParam(typed_c);
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
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static byte ScalarReturn()
    {
        return (byte)C1.ScalarReturn();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void ScalarParam([JSMarshalAs<JSType.Number>] byte c)
    {
        Color typed_c = (Color)c;
        C1.ScalarParam(typed_c);
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
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static short ScalarReturn()
    {
        return (short)C1.ScalarReturn();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void ScalarParam([JSMarshalAs<JSType.Number>] short c)
    {
        Color typed_c = (Color)c;
        C1.ScalarParam(typed_c);
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
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int ScalarReturn()
    {
        return (int)C1.ScalarReturn();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void ScalarParam([JSMarshalAs<JSType.Number>] int c)
    {
        Color typed_c = (Color)c;
        C1.ScalarParam(typed_c);
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
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static long ScalarReturn()
    {
        return (long)C1.ScalarReturn();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void ScalarParam([JSMarshalAs<JSType.Number>] long c)
    {
        Color typed_c = (Color)c;
        C1.ScalarParam(typed_c);
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
    [return: JSMarshalAs<JSType.Number>]
    public static long Echo([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] long b)
    {
        C1 typed_instance = (C1)instance;
        Big typed_b = (Big)b;
        return (long)typed_instance.Echo(typed_b);
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
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int? NullableReturn()
    {
        return (int?)C1.NullableReturn();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void NullableParam([JSMarshalAs<JSType.Number>] int? c)
    {
        Color? typed_c = c is { } cVal ? (Color)cVal : null;
        C1.NullableParam(typed_c);
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
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static int[] ArrayReturn()
    {
        return Array.ConvertAll(C1.ArrayReturn(), e => (int)e);
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void ArrayParam([JSMarshalAs<JSType.Array<JSType.Number>>] int[] c)
    {
        Color[] typed_c = Array.ConvertAll(c, e => (Color)e);
        C1.ArrayParam(typed_c);
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
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static Task<int> TaskReturn()
    {
        return C1.TaskReturn().ContinueWith(t => (int)t.Result, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
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

""");
    }

    private static string RenderInteropClassWithInitializer(string members)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public enum Color { Red, Green, Blue }
            [TSExport]
            public class C1
            {
            {{members}}
            }
        """.Replace("{{members}}", members));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedSymbols = [.. symbolExtractor.ExtractAllExportedSymbols()];
        INamedTypeSymbol classSymbol = exportedSymbols.First(s => s.Name == "C1");

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        return new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();
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
            Scalar = (Color)(initializer.GetPropertyAsInt32Nullable("Scalar") ?? throw new ArgumentException("Non-nullable property 'Scalar' missing or of invalid type", nameof(initializer))),
            Nullable = initializer.GetPropertyAsInt32Nullable("Nullable") is { } NullableVal ? (Color)NullableVal : null,
            Arr = Array.ConvertAll(initializer.GetPropertyAsInt32ArrayNullable("Arr") ?? throw new ArgumentException("Non-nullable property 'Arr' missing or of invalid type", nameof(initializer)), e => (Color)e),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_Scalar([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return (int)typed_instance.Scalar;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_Scalar([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] int value)
    {
        C1 typed_instance = (C1)instance;
        Color typed_value = (Color)value;
        typed_instance.Scalar = typed_value;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int? get_Nullable([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return (int?)typed_instance.Nullable;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_Nullable([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] int? value)
    {
        C1 typed_instance = (C1)instance;
        Color? typed_value = value is { } valueVal ? (Color)valueVal : null;
        typed_instance.Nullable = typed_value;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static int[] get_Arr([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return Array.ConvertAll(typed_instance.Arr, e => (int)e);
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_Arr([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Number>>] int[] value)
    {
        C1 typed_instance = (C1)instance;
        Color[] typed_value = Array.ConvertAll(value, e => (Color)e);
        typed_instance.Arr = typed_value;
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
            Scalar = (Color)(initializer.GetPropertyAsInt32Nullable("Scalar") ?? throw new ArgumentException("Non-nullable property 'Scalar' missing or of invalid type", nameof(initializer))),
            Nullable = initializer.GetPropertyAsInt32Nullable("Nullable") is { } NullableVal ? (Color)NullableVal : null,
            Arr = Array.ConvertAll(initializer.GetPropertyAsInt32ArrayNullable("Arr") ?? throw new ArgumentException("Non-nullable property 'Arr' missing or of invalid type", nameof(initializer)), e => (Color)e),
        };
    }
}

""");
    }
}
