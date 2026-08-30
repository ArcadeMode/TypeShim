using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_Enums
{
    private static string RenderInteropClass(string members)
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
                private C1() {}
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
}
