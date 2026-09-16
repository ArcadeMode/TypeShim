using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_NestedTypes
{
    private const string Source = """
        using System;
        using System.Threading.Tasks;
        namespace N1;
        [TSExport]
        public class Outer
        {
            private Outer() {}
            public static Inner Make(Kind k) => null;

            public enum Kind { A, B }

            public class Inner
            {
                private Inner() {}
                public static int Ping() => 1;
            }
        }

        [TSExport]
        public class Consumer
        {
            private Consumer() {}
            public static Outer.Inner Direct() => null;
            public static Outer.Inner[] Arr() => null;
            public static Outer.Inner? Nily() => null;
            public static Task<Outer.Inner> Asy() => null;
            public static Action<Outer.Inner> Del() => null;
            public static Outer.Kind EnumRef() => Outer.Kind.A;
        }
        """;

    private static string RenderInteropClass(string className)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(Source);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exported = [.. symbolExtractor.ExtractAllExportedSymbols()];
        InteropTypeInfoCache typeCache = new();
        List<NamedTypeInfo> named = [.. exported.Select(s => new NamedTypeInfoBuilder(s, typeCache).Build()).OfType<NamedTypeInfo>()];
        ClassInfo target = named.OfType<ClassInfo>().First(c => c.Name == className);
        RenderContext renderContext = new(target, named, RenderOptions.CSharp);
        new CSharpInteropClassRenderer(target, renderContext, new JSObjectMethodResolver([])).Render();
        return renderContext.ToString();
    }

    [Test]
    public void CSharpInteropClass_NestedClassAndEnum_RendersNestedInteropClass()
    {
        AssertEx.EqualOrDiff(RenderInteropClass("Outer"), """
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class OuterInterop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object Make([JSMarshalAs<JSType.Number>] int k)
    {
        global::N1.Outer.Kind typed_k = (global::N1.Outer.Kind)k;
        return (object)global::N1.Outer.Make(typed_k);
    }
    public static global::N1.Outer FromObject(object obj)
    {
        return obj switch
        {
            global::N1.Outer instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    public partial class InnerInterop
    {
        [JSExport]
        [return: JSMarshalAs<JSType.Number>]
        public static int Ping()
        {
            return global::N1.Outer.Inner.Ping();
        }
        public static global::N1.Outer.Inner FromObject(object obj)
        {
            return obj switch
            {
                global::N1.Outer.Inner instance => instance,
                _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
            };
        }
    }
}

""");
    }

    [Test]
    public void CSharpInteropClass_CrossClassNestedReferences_UseFullyQualifiedNestedInteropNames()
    {
        AssertEx.EqualOrDiff(RenderInteropClass("Consumer"), """
#nullable enable
// TypeShim generated TypeScript interop definitions
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class ConsumerInterop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object Direct()
    {
        return (object)global::N1.Consumer.Direct();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Any>>]
    public static object[] Arr()
    {
        return (object[])global::N1.Consumer.Arr();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object? Nily()
    {
        return (object?)global::N1.Consumer.Nily();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
    public static Task<object> Asy()
    {
        return global::N1.Consumer.Asy().ContinueWith(t => (object)t.Result, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Function<JSType.Any>>]
    public static Action<object> Del()
    {
        global::System.Action<global::N1.Outer.Inner> retVal = global::N1.Consumer.Del();
        return (object arg0) => retVal(global::N1.OuterInterop.InnerInterop.FromObject(arg0));
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int EnumRef()
    {
        return (int)global::N1.Consumer.EnumRef();
    }
    public static global::N1.Consumer FromObject(object obj)
    {
        return obj switch
        {
            global::N1.Consumer instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
}

""");
    }
}
