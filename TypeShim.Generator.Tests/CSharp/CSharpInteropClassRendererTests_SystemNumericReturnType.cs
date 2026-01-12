using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_SystemNumericReturnType
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
    public void CSharpInteropClass_StaticMethod_HasJSTypeNumber_ForNumericReturnType(string typeExpression, string interopTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public static class C1
            {
                public static {{typeExpression}} M1()
                {
                    return 1;
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeInfoCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeInfoCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

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
    [return: JSMarshalAs<JSType.Number>]
    public static {{typeExpression}} M1()
    {
        return C1.M1();
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
    public void CSharpInteropClass_StaticMethod_HasJSTypeNumber_ForNullableNumericReturnType(string typeExpression, string interopTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public static class C1
            {
                public static {{typeExpression}} M1()
                {
                    return 1;
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeInfoCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeInfoCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext).Render();

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
    [return: JSMarshalAs<JSType.Number>]
    public static {{typeExpression}} M1()
    {
        return C1.M1();
    }
}

""".Replace("{{typeExpression}}", interopTypeExpression)));
    }
}
