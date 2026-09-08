using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_Constructors
{
    [Test]
    public void CSharpInteropClass_ParameterlessConstructor_WithNoSettableProperties()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public int P1 => 1;
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
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object ctor()
    {
        return new C1();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return typed_instance.P1;
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
    public void CSharpInteropClass_ParameterizedConstructor_WithPrimitiveParameters()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1(int p1, double p2)
            {
                public int P1 => 1;
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
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object ctor([JSMarshalAs<JSType.Number>] int p1, [JSMarshalAs<JSType.Number>] double p2)
    {
        return new C1(p1, p2);
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return typed_instance.P1;
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
    public void CSharpInteropClass_ParameterlessConstructor_WithSettableProperties()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1()
            {
                public int P1 { get; set; }
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
            SetP1(instance, initializer.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer)));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] int value)
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
            SetP1(instance, initializer.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer)));
        }
        return instance;
    }
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, int value);
}

""");
    }

    [Test]
    public void CSharpInteropClass_ParameterlessConstructor_WithInitOnlyProperties()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1()
            {
                public int P1 { get; init; }
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
            SetP1(instance, initializer.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer)));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return typed_instance.P1;
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
            SetP1(instance, initializer.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer)));
        }
        return instance;
    }
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, int value);
}

""");
    }

    [Test]
    public void CSharpInteropClass_ParameterizedConstructor_WithSettableProperties()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1(int p1, double p2)
            {
                public int P1 { get; set; }
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
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object ctor([JSMarshalAs<JSType.Number>] int p1, [JSMarshalAs<JSType.Number>] double p2, [JSMarshalAs<JSType.Object>] JSObject initializer)
    {
        using var _ = initializer;
        var instance = CreateInstance(p1, p2);
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, initializer.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer)));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] int value)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
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
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance(int p1, double p2);
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, int value);
}

""");
    }

    [Test]
    public void CSharpInteropClass_ParameterizedConstructor_WithInitOnlyProperties()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1(int p1, double p2)
            {
                public int P1 { get; init; }
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
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object ctor([JSMarshalAs<JSType.Number>] int p1, [JSMarshalAs<JSType.Number>] double p2, [JSMarshalAs<JSType.Object>] JSObject initializer)
    {
        using var _ = initializer;
        var instance = CreateInstance(p1, p2);
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, initializer.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer)));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return typed_instance.P1;
    }
    public static C1 FromObject(object obj)
    {
        return obj switch
        {
            C1 instance => instance,
            _ => throw new ArgumentException($"Invalid object type {obj?.GetType().ToString() ?? "null"}", nameof(obj)),
        };
    }
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance(int p1, double p2);
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, int value);
}

""");
    }

    [Test]
    public void CSharpInteropClass_ParameterizedConstructor_WithNullableUserClass_ConvertsCorrectly()
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
            public class C1(MyClass? p1)
            {
                public int P1 => 1;
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
        using System.Runtime.InteropServices.JavaScript;
        using System.Threading.Tasks;
        namespace N1;
        public partial class C1Interop
        {
            [JSExport]
            [return: JSMarshalAs<JSType.Any>]
            public static object ctor([JSMarshalAs<JSType.Any>] object? p1)
            {
                MyClass? typed_p1 = p1 is { } p1Val ? MyClassInterop.FromObject(p1Val) : null;
                return new C1(typed_p1);
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Number>]
            public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = C1Interop.FromObject(instance);
                return typed_instance.P1;
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
    public void CSharpInteropClass_ParameterizedConstructor_WithUserClassAction_ConvertsCorrectly()
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
            public class C1(Action<MyClass> p1)
            {
                public int P1 => 1;
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
        using System.Runtime.InteropServices.JavaScript;
        using System.Threading.Tasks;
        namespace N1;
        public partial class C1Interop
        {
            [JSExport]
            [return: JSMarshalAs<JSType.Any>]
            public static object ctor([JSMarshalAs<JSType.Function<JSType.Any>>] Action<object> p1)
            {
                Action<MyClass> typed_p1 = (MyClass arg0) => p1(arg0);
                return new C1(typed_p1);
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Number>]
            public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = C1Interop.FromObject(instance);
                return typed_instance.P1;
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
    public void CSharpInteropClass_ParameterlessConstructor_WithUserClassPropertyThatIsNotInitializerCompatible_ConvertsCorrectly()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class UserClass
            {
                public int Id { get; private set; } // No public setter, so not initializer-compatible
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public UserClass P1 { get; set; } // public setter, but UserClass is not initializer-compatible
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

        // Mapping in ctor done with UserClassInterop.FromObject will not implement property initialization (validated by other tests)
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
            SetP1(instance, UserClassInterop.FromObject(initializer.GetPropertyAsObjectNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer))));
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
        UserClass typed_value = UserClassInterop.FromObject(value);
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
            SetP1(instance, UserClassInterop.FromObject(initializer.GetPropertyAsObjectNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer))));
        }
        return instance;
    }
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, UserClass value);
}

""");
    }


    [Test]
    public void CSharpInteropClass_Constructor_WithRequiredMemberInitializer_UsesUnsafeAccessorConstructor()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public required int P1 { get; set; }
                public string P2 { get; set; }
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
            SetP1(instance, initializer.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer)));
        }
        else
        {
            throw new ArgumentException("Required property 'P1' was not provided", nameof(initializer));
        }
        if (initializer.HasProperty("P2"))
        {
            SetP2(instance, initializer.GetPropertyAsStringNullable("P2") ?? throw new ArgumentException("Non-nullable property 'P2' missing or of invalid type", nameof(initializer)));
        }
        return instance;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] int value)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        typed_instance.P1 = value;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.String>]
    public static string get_P2([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
        return typed_instance.P2;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P2([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.String>] string value)
    {
        C1 typed_instance = C1Interop.FromObject(instance);
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
        var instance = CreateInstance();
        if (initializer.HasProperty("P1"))
        {
            SetP1(instance, initializer.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(initializer)));
        }
        else
        {
            throw new ArgumentException("Required property 'P1' was not provided", nameof(initializer));
        }
        if (initializer.HasProperty("P2"))
        {
            SetP2(instance, initializer.GetPropertyAsStringNullable("P2") ?? throw new ArgumentException("Non-nullable property 'P2' missing or of invalid type", nameof(initializer)));
        }
        return instance;
    }
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Constructor)]
    private static extern C1 CreateInstance();
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_P1")]
    private static extern void SetP1(C1 target, int value);
    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_P2")]
    private static extern void SetP2(C1 target, string value);
}

""");
    }
}
