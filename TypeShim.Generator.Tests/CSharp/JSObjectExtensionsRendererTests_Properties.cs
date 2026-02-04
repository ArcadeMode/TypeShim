using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.CSharp;

internal class JSObjectExtensionsRendererTests_Properties
{
    [TestCase("short", "Int16")]
    [TestCase("int", "Int32")]
    [TestCase("long", "Int64")]
    [TestCase("float", "Single")]
    [TestCase("double", "Double")]
    public void JSObjectExtensionsRendererTests_InstanceProperty_WithNumberType(string csTypeName, string managedSuffix)
    {
        string source = """
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public __TYPE__ P1 { get; set; }
            }
        """.Replace("__TYPE__", csTypeName);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        List<InteropTypeInfo> types = [classInfo.Properties.First().Type];
        RenderContext extensionsRenderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        new JSObjectExtensionsRenderer(extensionsRenderContext, types).Render();

        string expected = """    
        #nullable enable
        // JSImports for the type marshalling process
        using System;
        using System.Runtime.InteropServices.JavaScript;
        using System.Threading.Tasks;
        public static partial class JSObjectExtensions
        {
            public static __TYPE__? GetPropertyAs__MANAGED__Nullable(this JSObject jsObject, string propertyName)
            {
                return jsObject.HasProperty(propertyName) ? MarshalAs__MANAGED__(jsObject, propertyName) : null;
            }
            [JSImport("unwrapProperty", "@typeshim")]
            [return: JSMarshalAs<JSType.Number>]
            public static partial __TYPE__ MarshalAs__MANAGED__([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string propertyName);
        }
        
        """
        .Replace("__TYPE__", csTypeName)
        .Replace("__MANAGED__", managedSuffix);

        AssertEx.EqualOrDiff(extensionsRenderContext.ToString(), expected);
    }

    [Test]
    public void JSObjectExtensionsRendererTests_InstanceProperty_WithUserClassType()
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

        List<InteropTypeInfo> types = [classInfo.Properties.First().Type];
        RenderContext extensionsRenderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        new JSObjectExtensionsRenderer(extensionsRenderContext, types).Render();
        AssertEx.EqualOrDiff(extensionsRenderContext.ToString(), """    
        #nullable enable
        // JSImports for the type marshalling process
        using System;
        using System.Runtime.InteropServices.JavaScript;
        using System.Threading.Tasks;
        public static partial class JSObjectExtensions
        {
            public static JSObject? GetPropertyAsJSObjectNullable(this JSObject jsObject, string propertyName)
            {
                return jsObject.HasProperty(propertyName) ? MarshalAsJSObject(jsObject, propertyName) : null;
            }
            [JSImport("unwrapProperty", "@typeshim")]
            [return: JSMarshalAs<JSType.Object>]
            public static partial JSObject MarshalAsJSObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string propertyName);
        }
        
        """);
    }

    [Test]
    public void JSObjectExtensionsRendererTests_InstanceProperty_WithUserClassArrayType()
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

        List<InteropTypeInfo> types = [classInfo.Properties.First().Type];
        RenderContext extensionsRenderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        new JSObjectExtensionsRenderer(extensionsRenderContext, types).Render();
        AssertEx.EqualOrDiff(extensionsRenderContext.ToString(), """    
        #nullable enable
        // JSImports for the type marshalling process
        using System;
        using System.Runtime.InteropServices.JavaScript;
        using System.Threading.Tasks;
        public static partial class JSObjectExtensions
        {
            public static JSObject[]? GetPropertyAsJSObjectArrayNullable(this JSObject jsObject, string propertyName)
            {
                return jsObject.HasProperty(propertyName) ? MarshalAsJSObjectArray(jsObject, propertyName) : null;
            }
            [JSImport("unwrapProperty", "@typeshim")]
            [return: JSMarshalAs<JSType.Array<JSType.Object>>]
            public static partial JSObject[] MarshalAsJSObjectArray([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string propertyName);
        }
        
        """);
    }
}
