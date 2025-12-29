using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_SystemNumericParameterType
{
    [TestCase("Byte", "byte")]
    [TestCase("byte", "byte")]
    [TestCase("Int16", "short")]
    [TestCase("short", "short")]
    [TestCase("Int32", "int")]
    [TestCase("int", "int")]
    [TestCase("Int64", "long")]
    [TestCase("long", "long")]
    [TestCase("Single", "float")]
    [TestCase("float", "float")]
    [TestCase("Double", "double")]
    [TestCase("double", "double")]
    [TestCase("IntPtr", "nint")]
    [TestCase("nint", "nint")]
    public void CSharpInteropClass_StaticMethod_RenderedWithJSTypeNumber_ForSupportedNumericParameterType(string typeExpression, string interopTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public static class C1
            {
                public static void M1({{typeExpression}} arg1)
                {
                    var x = arg1;
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        RenderContext renderContext = new(classInfo, [classInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Number>] {{typeExpression}} arg1)
    {
        C1.M1(arg1);
    }
}

""".Replace("{{typeExpression}}", interopTypeExpression)));
    }

    [TestCase("Byte?", "byte?")]
    [TestCase("byte?", "byte?")]
    [TestCase("Int16?", "short?")]
    [TestCase("short?", "short?")]
    [TestCase("Int32?", "int?")]
    [TestCase("int?", "int?")]
    [TestCase("Int64?", "long?")]
    [TestCase("long?", "long?")]
    [TestCase("Single?", "float?")]
    [TestCase("float?", "float?")]
    [TestCase("Double?", "double?")]
    [TestCase("double?", "double?")]
    [TestCase("IntPtr?", "nint?")]
    [TestCase("nint?", "nint?")]
    public void CSharpInteropClass_StaticMethod_RenderedWithJSTypeNumber_ForSupportedNullableNumericParameterType(string typeExpression, string interopTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public static class C1
            {
                public static void M1({{typeExpression}} arg1)
                {
                    var x = arg1;
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        RenderContext renderContext = new(classInfo, [classInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Number>] {{typeExpression}} arg1)
    {
        C1.M1(arg1);
    }
}

""".Replace("{{typeExpression}}", interopTypeExpression)));
    }

    [TestCase("Byte", "byte")]
    [TestCase("byte", "byte")]
    [TestCase("Int16", "short")]
    [TestCase("short", "short")]
    [TestCase("Int32", "int")]
    [TestCase("int", "int")]
    [TestCase("Int64", "long")]
    [TestCase("long", "long")]
    [TestCase("Single", "float")]
    [TestCase("float", "float")]
    [TestCase("Double", "double")]
    [TestCase("double", "double")]
    [TestCase("IntPtr", "nint")]
    [TestCase("nint", "nint")]
    public void CSharpInteropClass_InstanceMethod_RenderedWithJSTypeNumber_ForSupportedNumericParameterType(string typeExpression, string interopTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                private C1() {}
                public void M1({{typeExpression}} arg1)
                {
                    var x = arg1;
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        RenderContext renderContext = new(classInfo, [classInfo], indentSpaces: 4);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Number>] {{typeExpression}} arg1)
    {
        C1 typed_instance = (C1)instance;
        typed_instance.M1(arg1);
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

""".Replace("{{typeExpression}}", interopTypeExpression)));
    }
}
