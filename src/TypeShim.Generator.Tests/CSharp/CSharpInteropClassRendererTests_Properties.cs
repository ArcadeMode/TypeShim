using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
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
    public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
    {
        using var _ = jsObject;
        return new C1()
        {
            P1 = MyClassInterop.FromObject(jsObject.GetPropertyAsObjectNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject))),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return (object)typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object value)
    {
        C1 typed_instance = (C1)instance;
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
    public static C1 FromJSObject(JSObject jsObject)
    {
        using var _ = jsObject;
        return new C1()
        {
            P1 = MyClassInterop.FromObject(jsObject.GetPropertyAsObjectNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject))),
        };
    }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
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
            public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
            {
                using var _ = jsObject;
                return new C1()
                {
                    P1 = jsObject.GetPropertyAsObjectNullable("P1") is { } P1Val ? MyClassInterop.FromObject(P1Val) : null,
                };
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Any>]
            public static object? get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = (C1)instance;
                return (object?)typed_instance.P1;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Void>]
            public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object? value)
            {
                C1 typed_instance = (C1)instance;
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
            public static C1 FromJSObject(JSObject jsObject)
            {
                using var _ = jsObject;
                return new C1()
                {
                    P1 = jsObject.GetPropertyAsObjectNullable("P1") is { } P1Val ? MyClassInterop.FromObject(P1Val) : null,
                };
            }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
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
    public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
    {
        using var _ = jsObject;
        return new C1()
        {
            P1 = jsObject.GetPropertyAsObjectNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object value)
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
        using var _ = jsObject;
        return new C1()
        {
            P1 = jsObject.GetPropertyAsObjectNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
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
    public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
    {
        using var _ = jsObject;
        return new C1()
        {
            P1 = jsObject.GetPropertyAsInt32ArrayNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static int[] get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Number>>] int[] value)
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
        using var _ = jsObject;
        return new C1()
        {
            P1 = jsObject.GetPropertyAsInt32ArrayNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
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
            public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
            {
                using var _ = jsObject;
                return new C1()
                {
                    P1 = jsObject.GetPropertyAsInt32ArraySegmentNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
                };
            }
            [JSExport]
            [return: JSMarshalAs<JSType.MemoryView>]
            public static ArraySegment<int> get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = (C1)instance;
                return typed_instance.P1;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Void>]
            public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.MemoryView>] ArraySegment<int> value)
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
                using var _ = jsObject;
                return new C1()
                {
                    P1 = jsObject.GetPropertyAsInt32ArraySegmentNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
                };
            }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
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
    public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
    {
        using var _ = jsObject;
        return new C1()
        {
            P1 = Array.ConvertAll(jsObject.GetPropertyAsObjectArrayNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)), e => MyClassInterop.FromObject(e)),
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
    public static C1 FromJSObject(JSObject jsObject)
    {
        using var _ = jsObject;
        return new C1()
        {
            P1 = Array.ConvertAll(jsObject.GetPropertyAsObjectArrayNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)), e => MyClassInterop.FromObject(e)),
        };
    }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
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
            public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
            {
                using var _ = jsObject;
                return new C1()
                {
                    P1 = Array.ConvertAll(jsObject.GetPropertyAsObjectNullableArrayNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)), e => e is { } eVal ? MyClassInterop.FromObject(eVal) : null),
                };
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Array<JSType.Any>>]
            public static object?[] get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = (C1)instance;
                return (object?[])typed_instance.P1;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Void>]
            public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object?[] value)
            {
                C1 typed_instance = (C1)instance;
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
            public static C1 FromJSObject(JSObject jsObject)
            {
                using var _ = jsObject;
                return new C1()
                {
                    P1 = Array.ConvertAll(jsObject.GetPropertyAsObjectNullableArrayNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)), e => e is { } eVal ? MyClassInterop.FromObject(eVal) : null),
                };
            }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
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
            public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
            {
                using var _ = jsObject;
                return new C1()
                {
                    P1 = jsObject.GetPropertyAsObjectArrayNullable("P1") is { } P1Val ? Array.ConvertAll(P1Val, e => MyClassInterop.FromObject(e)) : null,
                };
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Array<JSType.Any>>]
            public static object[]? get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = (C1)instance;
                return (object[]?)typed_instance.P1;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Void>]
            public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object[]? value)
            {
                C1 typed_instance = (C1)instance;
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
            public static C1 FromJSObject(JSObject jsObject)
            {
                using var _ = jsObject;
                return new C1()
                {
                    P1 = jsObject.GetPropertyAsObjectArrayNullable("P1") is { } P1Val ? Array.ConvertAll(P1Val, e => MyClassInterop.FromObject(e)) : null,
                };
            }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
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
    public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
    {
        using var _ = jsObject;
        return new C1()
        {
            P1 = jsObject.GetPropertyAsObjectNullableArrayNullable("P1") is { } P1Val ? Array.ConvertAll(P1Val, e => e is { } eVal ? MyClassInterop.FromObject(eVal) : null) : null,
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Any>>]
    public static object?[]? get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return (object?[]?)typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Array<JSType.Any>>] object?[]? value)
    {
        C1 typed_instance = (C1)instance;
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
    public static C1 FromJSObject(JSObject jsObject)
    {
        using var _ = jsObject;
        return new C1()
        {
            P1 = jsObject.GetPropertyAsObjectNullableArrayNullable("P1") is { } P1Val ? Array.ConvertAll(P1Val, e => e is { } eVal ? MyClassInterop.FromObject(eVal) : null) : null,
        };
    }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
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
    public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
    {
        using var _ = jsObject;
        TaskCompletionSource<MyClass> P1Tcs = new();
        (jsObject.GetPropertyAsObjectTaskNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject))).ContinueWith(t => {
            if (t.IsFaulted) P1Tcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) P1Tcs.SetCanceled();
            else P1Tcs.SetResult(MyClassInterop.FromObject(t.Result));
        }, TaskContinuationOptions.ExecuteSynchronously);
        return new C1()
        {
            P1 = P1Tcs.Task,
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
        TaskCompletionSource<MyClass> valueTcs = new();
        (value).ContinueWith(t => {
            if (t.IsFaulted) valueTcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) valueTcs.SetCanceled();
            else valueTcs.SetResult(MyClassInterop.FromObject(t.Result));
        }, TaskContinuationOptions.ExecuteSynchronously);
        Task<MyClass> typed_value = valueTcs.Task;
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
    public static C1 FromJSObject(JSObject jsObject)
    {
        using var _ = jsObject;
        TaskCompletionSource<MyClass> P1Tcs = new();
        (jsObject.GetPropertyAsObjectTaskNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject))).ContinueWith(t => {
            if (t.IsFaulted) P1Tcs.SetException(t.Exception.InnerExceptions);
            else if (t.IsCanceled) P1Tcs.SetCanceled();
            else P1Tcs.SetResult(MyClassInterop.FromObject(t.Result));
        }, TaskContinuationOptions.ExecuteSynchronously);
        return new C1()
        {
            P1 = P1Tcs.Task,
        };
    }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
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
            public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
            {
                using var _ = jsObject;
                TaskCompletionSource<MyClass?> P1Tcs = new();
                (jsObject.GetPropertyAsObjectNullableTaskNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject))).ContinueWith(t => {
                    if (t.IsFaulted) P1Tcs.SetException(t.Exception.InnerExceptions);
                    else if (t.IsCanceled) P1Tcs.SetCanceled();
                    else P1Tcs.SetResult(t.Result is { } P1TcsVal ? MyClassInterop.FromObject(P1TcsVal) : null);
                }, TaskContinuationOptions.ExecuteSynchronously);
                return new C1()
                {
                    P1 = P1Tcs.Task,
                };
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
            public static Task<object?> get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = (C1)instance;
                TaskCompletionSource<object?> retValTcs = new();
                (typed_instance.P1).ContinueWith(t => {
                    if (t.IsFaulted) retValTcs.SetException(t.Exception.InnerExceptions);
                    else if (t.IsCanceled) retValTcs.SetCanceled();
                    else retValTcs.SetResult(t.Result is { } retValTcsVal ? (object)retValTcsVal : null);
                }, TaskContinuationOptions.ExecuteSynchronously);
                return retValTcs.Task;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Void>]
            public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Promise<JSType.Any>>] Task<object?> value)
            {
                C1 typed_instance = (C1)instance;
                TaskCompletionSource<MyClass?> valueTcs = new();
                (value).ContinueWith(t => {
                    if (t.IsFaulted) valueTcs.SetException(t.Exception.InnerExceptions);
                    else if (t.IsCanceled) valueTcs.SetCanceled();
                    else valueTcs.SetResult(t.Result is { } valueTcsVal ? MyClassInterop.FromObject(valueTcsVal) : null);
                }, TaskContinuationOptions.ExecuteSynchronously);
                Task<MyClass?> typed_value = valueTcs.Task;
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
            public static C1 FromJSObject(JSObject jsObject)
            {
                using var _ = jsObject;
                TaskCompletionSource<MyClass?> P1Tcs = new();
                (jsObject.GetPropertyAsObjectNullableTaskNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject))).ContinueWith(t => {
                    if (t.IsFaulted) P1Tcs.SetException(t.Exception.InnerExceptions);
                    else if (t.IsCanceled) P1Tcs.SetCanceled();
                    else P1Tcs.SetResult(t.Result is { } P1TcsVal ? MyClassInterop.FromObject(P1TcsVal) : null);
                }, TaskContinuationOptions.ExecuteSynchronously);
                return new C1()
                {
                    P1 = P1Tcs.Task,
                };
            }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
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
            public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
            {
                using var _ = jsObject;
                TaskCompletionSource<MyClass?>? P1Tcs = jsObject.GetPropertyAsObjectNullableTaskNullable("P1") != null ? new() : null;
                jsObject.GetPropertyAsObjectNullableTaskNullable("P1")?.ContinueWith(t => {
                    if (t.IsFaulted) P1Tcs!.SetException(t.Exception.InnerExceptions);
                    else if (t.IsCanceled) P1Tcs!.SetCanceled();
                    else P1Tcs!.SetResult(t.Result is { } P1TcsVal ? MyClassInterop.FromObject(P1TcsVal) : null);
                }, TaskContinuationOptions.ExecuteSynchronously);
                return new C1()
                {
                    P1 = P1Tcs?.Task,
                };
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
            public static Task<object?>? get_P1([JSMarshalAs<JSType.Any>] object instance)
            {
                C1 typed_instance = (C1)instance;
                TaskCompletionSource<object?>? retValTcs = typed_instance.P1 != null ? new() : null;
                typed_instance.P1?.ContinueWith(t => {
                    if (t.IsFaulted) retValTcs!.SetException(t.Exception.InnerExceptions);
                    else if (t.IsCanceled) retValTcs!.SetCanceled();
                    else retValTcs!.SetResult(t.Result is { } retValTcsVal ? (object)retValTcsVal : null);
                }, TaskContinuationOptions.ExecuteSynchronously);
                return retValTcs?.Task;
            }
            [JSExport]
            [return: JSMarshalAs<JSType.Void>]
            public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Promise<JSType.Any>>] Task<object?>? value)
            {
                C1 typed_instance = (C1)instance;
                TaskCompletionSource<MyClass?>? valueTcs = value != null ? new() : null;
                value?.ContinueWith(t => {
                    if (t.IsFaulted) valueTcs!.SetException(t.Exception.InnerExceptions);
                    else if (t.IsCanceled) valueTcs!.SetCanceled();
                    else valueTcs!.SetResult(t.Result is { } valueTcsVal ? MyClassInterop.FromObject(valueTcsVal) : null);
                }, TaskContinuationOptions.ExecuteSynchronously);
                Task<MyClass?>? typed_value = valueTcs?.Task;
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
            public static C1 FromJSObject(JSObject jsObject)
            {
                using var _ = jsObject;
                TaskCompletionSource<MyClass?>? P1Tcs = jsObject.GetPropertyAsObjectNullableTaskNullable("P1") != null ? new() : null;
                jsObject.GetPropertyAsObjectNullableTaskNullable("P1")?.ContinueWith(t => {
                    if (t.IsFaulted) P1Tcs!.SetException(t.Exception.InnerExceptions);
                    else if (t.IsCanceled) P1Tcs!.SetCanceled();
                    else P1Tcs!.SetResult(t.Result is { } P1TcsVal ? MyClassInterop.FromObject(P1TcsVal) : null);
                }, TaskContinuationOptions.ExecuteSynchronously);
                return new C1()
                {
                    P1 = P1Tcs?.Task,
                };
            }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
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
    public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
    {
        using var _ = jsObject;
        return new C1()
        {
            P1 = ({{typeName}}[])jsObject.GetPropertyAsObjectArrayNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
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
        using var _ = jsObject;
        return new C1()
        {
            P1 = ({{typeName}}[])jsObject.GetPropertyAsObjectArrayNullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
}

""".Replace("{{typeName}}", typeName));
    }
}
