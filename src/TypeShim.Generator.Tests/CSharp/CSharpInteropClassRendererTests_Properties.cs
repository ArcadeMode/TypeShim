using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, MyClassInterop.FromObject(initializer.GetObjectProperty("P1")));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return (object)typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object value)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
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
    public static C1 FromJSObject(JSObject initializer)
    {
        using var _ = initializer;
        var instance = CreateInstance();
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, MyClassInterop.FromObject(initializer.GetObjectProperty("P1")));
        }
        return instance;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, MyClass value);
}

""");
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithUserClassType_InDifferentNamespace_QualifiesReferences()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N2;
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
                public N2.MyClass P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First(c => c.Name == "C1");

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.First(c => c.Name == "MyClass"), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

        // C1 (own namespace N1) references stay minimally qualified; the cross-namespace
        // MyClass (N2) type and its interop class are qualified with global:: so the generated
        // C# compiles without a using directive for N2.
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
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, global::N2.MyClassInterop.FromObject(initializer.GetObjectProperty("P1")));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return (object)typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object value)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        global::N2.MyClass typed_value = global::N2.MyClassInterop.FromObject(value);
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
    public static C1 FromJSObject(JSObject initializer)
    {
        using var _ = initializer;
        var instance = CreateInstance();
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, global::N2.MyClassInterop.FromObject(initializer.GetObjectProperty("P1")));
        }
        return instance;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, global::N2.MyClass value);
}

""");
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithNullableUserClassType_InDifferentNamespace_QualifiesReferences()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N2;
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
                public N2.MyClass? P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First(c => c.Name == "C1");

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.First(c => c.Name == "MyClass"), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

        // The cross-namespace nullable reference keeps its `?` annotation while being qualified with
        // global::, so the generated C# stays nullability-correct and compiles without a using for N2.
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
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, initializer.GetObjectNullableProperty("P1") is { } P1Val ? global::N2.MyClassInterop.FromObject(P1Val) : null);
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object? get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return (object?)typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object? value)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        global::N2.MyClass? typed_value = value is { } valueVal ? global::N2.MyClassInterop.FromObject(valueVal) : null;
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
    public static C1 FromJSObject(JSObject initializer)
    {
        using var _ = initializer;
        var instance = CreateInstance();
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, initializer.GetObjectNullableProperty("P1") is { } P1Val ? global::N2.MyClassInterop.FromObject(P1Val) : null);
        }
        return instance;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, global::N2.MyClass? value);
}

""");
    }

    [Test]
    public void CSharpInteropClass_StaticProperty_WithUserClassType()
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
            public static class C1
            {
                public static MyClass P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
    public static object get_P1()
    {
        return (object)C1.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object value)
    {
        MyClass typed_value = MyClassInterop.FromObject(value);
        C1.P1 = typed_value;
    }
}

""");
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
                if (initializer.HasProperty("P1"))
                {
                    SetP1(instance, initializer.GetObjectNullableProperty("P1") is { } P1Val ? MyClassInterop.FromObject(P1Val) : null);
                }
                return instance;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Any>]
            public static object? get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = C1Interop.FromObject(instance);
                return (object?)typed_instance.P1;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Void>]
            public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object? value)
            {
                C1 typed_instance = C1Interop.FromObject(instance);
                MyClass? typed_value = value is { } valueVal ? MyClassInterop.FromObject(valueVal) : null;
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
            public static C1 FromJSObject(JSObject initializer)
            {
                using var _ = initializer;
                var instance = CreateInstance();
                if (initializer.HasProperty("P1"))
                {
                    SetP1(instance, initializer.GetObjectNullableProperty("P1") is { } P1Val ? MyClassInterop.FromObject(P1Val) : null);
                }
                return instance;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
            private static extern C1 CreateInstance();

            [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
            private static extern void SetP1(C1 target, MyClass? value);
        }

        """);
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, initializer.GetObjectProperty("P1"));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object value)
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
            SetP1(instance, initializer.GetObjectProperty("P1"));
        }
        return instance;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, object value);
}

""");
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, initializer.GetInt32ArrayProperty("P1"));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static int[] get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Number>>] int[] value)
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
            SetP1(instance, initializer.GetInt32ArrayProperty("P1"));
        }
        return instance;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, int[] value);
}

""");
    }
    
    [Test]
    public void CSharpInteropClass_InstanceProperty_WithIntArraySegmentType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public ArraySegment<int> P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
                if (initializer.HasProperty("P1"))
                {
                    SetP1(instance, initializer.GetInt32ArraySegmentProperty("P1"));
                }
                return instance;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.MemoryView>]
            public static ArraySegment<int> get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = C1Interop.FromObject(instance);
                return typed_instance.P1;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Void>]
            public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.MemoryView>] ArraySegment<int> value)
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
                    SetP1(instance, initializer.GetInt32ArraySegmentProperty("P1"));
                }
                return instance;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
            private static extern C1 CreateInstance();

            [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
            private static extern void SetP1(C1 target, ArraySegment<int> value);
        }

        """);
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, Array.ConvertAll(initializer.GetObjectArrayProperty("P1"), e => MyClassInterop.FromObject(e)));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Any>>]
    public static object[] get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return (object[])typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object[] value)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
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
    public static C1 FromJSObject(JSObject initializer)
    {
        using var _ = initializer;
        var instance = CreateInstance();
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, Array.ConvertAll(initializer.GetObjectArrayProperty("P1"), e => MyClassInterop.FromObject(e)));
        }
        return instance;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, MyClass[] value);
}

""");
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
                if (initializer.HasProperty("P1"))
                {
                    SetP1(instance, Array.ConvertAll(initializer.GetObjectNullableArrayProperty("P1"), e => e is { } eVal ? MyClassInterop.FromObject(eVal) : null));
                }
                return instance;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Array<JSType.Any>>]
            public static object?[] get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = C1Interop.FromObject(instance);
                return (object?[])typed_instance.P1;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Void>]
            public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object?[] value)
            {
                C1 typed_instance = C1Interop.FromObject(instance);
                MyClass?[] typed_value = Array.ConvertAll(value, e => e is { } eVal ? MyClassInterop.FromObject(eVal) : null);
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
            public static C1 FromJSObject(JSObject initializer)
            {
                using var _ = initializer;
                var instance = CreateInstance();
                if (initializer.HasProperty("P1"))
                {
                    SetP1(instance, Array.ConvertAll(initializer.GetObjectNullableArrayProperty("P1"), e => e is { } eVal ? MyClassInterop.FromObject(eVal) : null));
                }
                return instance;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
            private static extern C1 CreateInstance();

            [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
            private static extern void SetP1(C1 target, MyClass?[] value);
        }

        """);
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();
        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
                if (initializer.HasProperty("P1"))
                {
                    SetP1(instance, initializer.GetObjectArrayNullableProperty("P1") is { } P1Val ? Array.ConvertAll(P1Val, e => MyClassInterop.FromObject(e)) : null);
                }
                return instance;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Array<JSType.Any>>]
            public static object[]? get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = C1Interop.FromObject(instance);
                return (object[]?)typed_instance.P1;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Void>]
            public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object[]? value)
            {
                C1 typed_instance = C1Interop.FromObject(instance);
                MyClass[]? typed_value = value is { } valueVal ? Array.ConvertAll(valueVal, e => MyClassInterop.FromObject(e)) : null;
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
            public static C1 FromJSObject(JSObject initializer)
            {
                using var _ = initializer;
                var instance = CreateInstance();
                if (initializer.HasProperty("P1"))
                {
                    SetP1(instance, initializer.GetObjectArrayNullableProperty("P1") is { } P1Val ? Array.ConvertAll(P1Val, e => MyClassInterop.FromObject(e)) : null);
                }
                return instance;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
            private static extern C1 CreateInstance();

            [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
            private static extern void SetP1(C1 target, MyClass[]? value);
        }

        """);
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, initializer.GetObjectNullableArrayNullableProperty("P1") is { } P1Val ? Array.ConvertAll(P1Val, e => e is { } eVal ? MyClassInterop.FromObject(eVal) : null) : null);
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Any>>]
    public static object?[]? get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return (object?[]?)typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object?[]? value)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        MyClass?[]? typed_value = value is { } valueVal ? Array.ConvertAll(valueVal, e => e is { } eVal ? MyClassInterop.FromObject(eVal) : null) : null;
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
    public static C1 FromJSObject(JSObject initializer)
    {
        using var _ = initializer;
        var instance = CreateInstance();
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, initializer.GetObjectNullableArrayNullableProperty("P1") is { } P1Val ? Array.ConvertAll(P1Val, e => e is { } eVal ? MyClassInterop.FromObject(eVal) : null) : null);
        }
        return instance;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, MyClass?[]? value);
}

""");
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, initializer.GetObjectTaskProperty("P1").ContinueWith(t => MyClassInterop.FromObject(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
    public static Task<object> get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return typed_instance.P1.ContinueWith(t => (object)t.Result, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Promise<JSType.Any>>] Task<object> value)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        Task<MyClass> typed_value = value.ContinueWith(t => MyClassInterop.FromObject(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
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
    public static C1 FromJSObject(JSObject initializer)
    {
        using var _ = initializer;
        var instance = CreateInstance();
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, initializer.GetObjectTaskProperty("P1").ContinueWith(t => MyClassInterop.FromObject(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously));
        }
        return instance;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, Task<MyClass> value);
}

""");
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
                if (initializer.HasProperty("P1"))
                {
                    SetP1(instance, initializer.GetObjectNullableTaskProperty("P1").ContinueWith(t => t.Result is { } tVal ? MyClassInterop.FromObject(tVal) : null, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously));
                }
                return instance;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
            public static Task<object?> get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = C1Interop.FromObject(instance);
                return typed_instance.P1.ContinueWith(t => (object?)t.Result, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Void>]
            public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Promise<JSType.Any>>] Task<object?> value)
            {
                C1 typed_instance = C1Interop.FromObject(instance);
                Task<MyClass?> typed_value = value.ContinueWith(t => t.Result is { } tVal ? MyClassInterop.FromObject(tVal) : null, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
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
            public static C1 FromJSObject(JSObject initializer)
            {
                using var _ = initializer;
                var instance = CreateInstance();
                if (initializer.HasProperty("P1"))
                {
                    SetP1(instance, initializer.GetObjectNullableTaskProperty("P1").ContinueWith(t => t.Result is { } tVal ? MyClassInterop.FromObject(tVal) : null, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously));
                }
                return instance;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
            private static extern C1 CreateInstance();

            [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
            private static extern void SetP1(C1 target, Task<MyClass?> value);
        }

        """);
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

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
                if (initializer.HasProperty("P1"))
                {
                    SetP1(instance, initializer.GetObjectNullableTaskNullableProperty("P1")?.ContinueWith(t => t.Result is { } tVal ? MyClassInterop.FromObject(tVal) : null, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously));
                }
                return instance;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
            public static Task<object?>? get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = C1Interop.FromObject(instance);
                return typed_instance.P1?.ContinueWith(t => (object?)t.Result, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Void>]
            public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Promise<JSType.Any>>] Task<object?>? value)
            {
                C1 typed_instance = C1Interop.FromObject(instance);
                Task<MyClass?>? typed_value = value?.ContinueWith(t => t.Result is { } tVal ? MyClassInterop.FromObject(tVal) : null, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
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
            public static C1 FromJSObject(JSObject initializer)
            {
                using var _ = initializer;
                var instance = CreateInstance();
                if (initializer.HasProperty("P1"))
                {
                    SetP1(instance, initializer.GetObjectNullableTaskNullableProperty("P1")?.ContinueWith(t => t.Result is { } tVal ? MyClassInterop.FromObject(tVal) : null, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously));
                }
                return instance;
            }

            [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
            private static extern C1 CreateInstance();

            [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
            private static extern void SetP1(C1 target, Task<MyClass?>? value);
        }

        """);
    }

    [TestCase("Version")]
    [TestCase("Uri")]
    public void CSharpInteropClass_InstanceProperty_WithNonUserClassArrayType_ConvertsWithCast_InConstructorAndFromJSObjectMethod(string typeName) //i.e. is not snapshot compatible
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
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, ({{typeName}}[])initializer.GetObjectArrayProperty("P1"));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Any>>]
    public static object[] get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return (object[])typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object[] value)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        {{typeName}}[] typed_value = ({{typeName}}[])value;
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
    public static C1 FromJSObject(JSObject initializer)
    {
        using var _ = initializer;
        var instance = CreateInstance();
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, ({{typeName}}[])initializer.GetObjectArrayProperty("P1"));
        }
        return instance;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, {{typeName}}[] value);
}

""".Replace("{{typeName}}", typeName));
    }
}
