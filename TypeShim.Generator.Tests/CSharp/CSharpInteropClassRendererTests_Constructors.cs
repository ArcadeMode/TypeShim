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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver()).Render();

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
        C1 typed_instance = (C1)instance;
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver()).Render();

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
        C1 typed_instance = (C1)instance;
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver()).Render();

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
        return new C1()
        {
            P1 = jsObject.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] int value)
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
        return new C1()
        {
            P1 = jsObject.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver()).Render();

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
        return new C1()
        {
            P1 = jsObject.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
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
    public static C1 FromJSObject(JSObject jsObject)
    {
        return new C1()
        {
            P1 = jsObject.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver()).Render();

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
    public static object ctor([JSMarshalAs<JSType.Number>] int p1, [JSMarshalAs<JSType.Number>] double p2, [JSMarshalAs<JSType.Object>] JSObject jsObject)
    {
        return new C1(p1, p2)
        {
            P1 = jsObject.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] int value)
    {
        C1 typed_instance = (C1)instance;
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver()).Render();

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
    public static object ctor([JSMarshalAs<JSType.Number>] int p1, [JSMarshalAs<JSType.Number>] double p2, [JSMarshalAs<JSType.Object>] JSObject jsObject)
    {
        return new C1(p1, p2)
        {
            P1 = jsObject.GetPropertyAsInt32Nullable("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver()).Render();

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
        MyClass? typed_p1 = p1 != null ? MyClassInterop.FromObject(p1) : null;
        return new C1(typed_p1);
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver()).Render();

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
    public static object ctor([JSMarshalAs<JSType.Object>] JSObject jsObject)
    {
        return new C1()
        {
            P1 = UserClassInterop.FromObject(jsObject.GetPropertyAsJSObject("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject))),
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
    public static C1 FromJSObject(JSObject jsObject)
    {
        return new C1()
        {
            P1 = UserClassInterop.FromObject(jsObject.GetPropertyAsJSObject("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject))),
        };
    }
}

""");
    }

}
