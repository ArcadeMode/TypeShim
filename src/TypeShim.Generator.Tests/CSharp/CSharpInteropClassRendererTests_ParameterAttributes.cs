using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_ParameterAttributes
{
    private static string Render(string classBody)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText($$"""
            using System;
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;
            namespace N1;
            [TSExport]
            public class C1
            {
                private C1() {}
            {{classBody}}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses[0], typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        return new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();
    }

    [Test]
    public void OptionalAndDefaultParameterValue_InteropAlwaysPassesArgument()
    {
        string output = Render("    public void M1([Optional, DefaultParameterValue(19.99)] double price) {}");

        AssertEx.EqualOrDiff(output, """
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
    public static void M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] double price)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        typed_instance.M1(price);
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
    public void CallerMemberName_InteropAlwaysPassesArgument()
    {
        string output = Render("    public void M1(string message, [CallerMemberName] string caller = \"\") {}");

        AssertEx.EqualOrDiff(output, """
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
    public static void M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.String>] string message, [JSMarshalAs<JSType.String>] string caller)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        typed_instance.M1(message, caller);
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
    public void CallerLineNumber_InteropAlwaysPassesArgument()
    {
        string output = Render("    public void M1(string message, [CallerLineNumber] int line = 0) {}");

        AssertEx.EqualOrDiff(output, """
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
    public static void M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.String>] string message, [JSMarshalAs<JSType.Number>] int line)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        typed_instance.M1(message, line);
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
    public void CallerArgumentExpression_InteropAlwaysPassesArgument()
    {
        string output = Render("    public void M1(string value, [CallerArgumentExpression(nameof(value))] string? expr = null) {}");

        AssertEx.EqualOrDiff(output, """
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
    public static void M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.String>] string value, [JSMarshalAs<JSType.String>] string? expr)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        typed_instance.M1(value, expr);
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
