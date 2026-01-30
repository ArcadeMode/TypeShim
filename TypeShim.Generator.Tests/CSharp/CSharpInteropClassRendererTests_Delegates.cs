using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_Delegates
{
    [Test]
    public void CSharpInteropClass_MethodWithAction0ParameterType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public void M1(Action callback) => callback();
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

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
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Function>] Action callback)
    {
        C1 typed_instance = (C1)instance;
        typed_instance.M1(callback);
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
    public void CSharpInteropClass_MethodWithAction1ParameterType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public void M1(Action<string> callback) => callback();
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

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
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Function<JSType.String>>] Action<string> callback)
    {
        C1 typed_instance = (C1)instance;
        typed_instance.M1(callback);
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
    public void CSharpInteropClass_MethodWithAction2ParameterType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public void M1(Action<string, int> callback) => callback();
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

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
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Function<JSType.String, JSType.Number>>] Action<string, int> callback)
    {
        C1 typed_instance = (C1)instance;
        typed_instance.M1(callback);
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
    public void CSharpInteropClass_MethodWithAction3ParameterType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public void M1(Action<string, int, bool> callback) => callback();
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

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
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Function<JSType.String, JSType.Number, JSType.Boolean>>] Action<string, int, bool> callback)
    {
        C1 typed_instance = (C1)instance;
        typed_instance.M1(callback);
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
    public void CSharpInteropClass_MethodWithFunc1ParameterType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public string M1(Func<string> callback) => callback();
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

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
    [return: JSMarshalAs<JSType.String>]
    public static string M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Function<JSType.String>>] Func<string> callback)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.M1(callback);
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
    public void CSharpInteropClass_MethodWithFunc2ParameterType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public string M1(Func<string, int> callback) => callback(1);
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

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
    [return: JSMarshalAs<JSType.String>]
    public static string M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Function<JSType.String, JSType.Number>>] Func<string, int> callback)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.M1(callback);
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
    public void CSharpInteropClass_MethodWithFunc3ParameterType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public string M1(Func<string, int, bool> callback) => callback(1, true);
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

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
    [return: JSMarshalAs<JSType.String>]
    public static string M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Function<JSType.String, JSType.Number, JSType.Boolean>>] Func<string, int, bool> callback)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.M1(callback);
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
