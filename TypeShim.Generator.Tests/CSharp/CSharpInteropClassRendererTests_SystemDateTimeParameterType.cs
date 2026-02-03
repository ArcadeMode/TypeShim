using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_SystemDateTimeParameterType
{
    [TestCase("DateTime", "global::System.DateTime")]
    [TestCase("DateTimeOffset", "global::System.DateTimeOffset")]
    public void CSharpInteropClass_StaticMethod_HasJSTypeDateTime_ForDateTimeParameterType(string typeName, string interopTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public static class C1
            {
                public static void M1({{typeName}} p1)
                {
                }
            }
        """.Replace("{{typeName}}", typeName));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeInfoCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeInfoCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver()).Render();

        Assert.That(interopClass, Is.EqualTo("""    
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
    public static void M1([JSMarshalAs<JSType.Date>] {{interopTypeExpression}} p1)
    {
        C1.M1(p1);
    }
}

""".Replace("{{interopTypeExpression}}", interopTypeExpression)));
    }

    [TestCase("DateTime", "global::System.DateTime")]
    [TestCase("DateTimeOffset", "global::System.DateTimeOffset")]
    public void CSharpInteropClass_InstanceMethod_HasJSTypeDate_ForDateTimeParameterType(string typeName, string interopTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                private C1() {}
                public void M1({{typeName}} p1)
                {
                }
            }
        """.Replace("{{typeName}}", typeName));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeInfoCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeInfoCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver()).Render();

        Assert.That(interopClass, Is.EqualTo("""    
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
    public static void M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Date>] {{interopTypeExpression}} p1)
    {
        C1 typed_instance = (C1)instance;
        typed_instance.M1(p1);
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

""".Replace("{{interopTypeExpression}}", interopTypeExpression)));
    }

    [TestCase("DateTime", "global::System.DateTime", "GetPropertyAsDateTimeNullable")]
    [TestCase("DateTimeOffset", "global::System.DateTimeOffset", "GetPropertyAsDateTimeOffsetNullable")]
    public void CSharpInteropClass_InstanceProperty_ForDateTimeParameterType(string typeName, string interopTypeExpression, string jsObjectMethod)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                public {{typeName}} P1 { get; set; }
            }
        """.Replace("{{typeName}}", typeName));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeInfoCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeInfoCache).Build();
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
            P1 = jsObject.{{jsObjectMethod}}("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Date>]
    public static {{typeExpression}} get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Date>] {{typeExpression}} value)
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
            P1 = jsObject.{{jsObjectMethod}}("P1") ?? throw new ArgumentException("Non-nullable property 'P1' missing or of invalid type", nameof(jsObject)),
        };
    }
}

""".Replace("{{typeExpression}}", interopTypeExpression).Replace("{{jsObjectMethod}}", jsObjectMethod));
    }
}
