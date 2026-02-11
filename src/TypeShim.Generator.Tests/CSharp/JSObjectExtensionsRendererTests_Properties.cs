using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.CSharp;

internal class JSObjectExtensionsRendererTests_Properties
{
    [TestCase("bool", "Boolean", "JSType.Boolean")]
    [TestCase("string", "String", "JSType.String")]
    [TestCase("char", "Char", "JSType.Number", Ignore = ".NET currently wrongly expects JSType.String for char, which indeed is marshalled as JSType.Number at runtime")]
    [TestCase("char", "Char", "JSType.String")]
    [TestCase("short", "Int16", "JSType.Number")]
    [TestCase("int", "Int32", "JSType.Number")]
    [TestCase("long", "Int64", "JSType.Number")]
    [TestCase("float", "Single", "JSType.Number")]
    [TestCase("double", "Double", "JSType.Number")]
    public void JSObjectExtensionsRendererTests_InstanceProperty_WithBooleanType(string csTypeName, string managedSuffix, string jsType)
    {
        string source = """
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public {{type}} P1 { get; set; }
            }
        """.Replace("{{type}}", csTypeName);

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
            public static {{type}}? GetPropertyAs{{managed}}Nullable(this JSObject jsObject, string propertyName)
            {
                return jsObject.HasProperty(propertyName) ? MarshalAs{{managed}}(jsObject, propertyName) : null;
            }
            [JSImport("unwrapProperty", "@typeshim")]
            [return: JSMarshalAs<{{jstype}}>]
            public static partial {{type}} MarshalAs{{managed}}([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string propertyName);
        }
        
        """
        .Replace("{{type}}", csTypeName)
        .Replace("{{managed}}", managedSuffix)
        .Replace("{{jstype}}", jsType);

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

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
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
            public static object? GetPropertyAsObjectNullable(this JSObject jsObject, string propertyName)
            {
                return jsObject.HasProperty(propertyName) ? MarshalAsObject(jsObject, propertyName) : null;
            }
            [JSImport("unwrapProperty", "@typeshim")]
            [return: JSMarshalAs<JSType.Any>]
            public static partial object MarshalAsObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string propertyName);
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
            public static object[]? GetPropertyAsObjectArrayNullable(this JSObject jsObject, string propertyName)
            {
                return jsObject.HasProperty(propertyName) ? MarshalAsObjectArray(jsObject, propertyName) : null;
            }
            [JSImport("unwrapProperty", "@typeshim")]
            [return: JSMarshalAs<JSType.Array<JSType.Any>>]
            public static partial object[] MarshalAsObjectArray([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string propertyName);
        }
        
        """);
    }

    [Test]
    public void JSObjectExtensionsRendererTests_InstanceProperty_WithActionType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public Action P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.First();

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();

        List<InteropTypeInfo> types = [classInfo.Properties.First().Type];
        RenderContext extensionsRenderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        new JSObjectExtensionsRenderer(extensionsRenderContext, types).Render();

        AssertEx.EqualOrDiff(extensionsRenderContext.ToString(), """    
        #nullable enable
        // JSImports for the type marshalling process
        using System;
        using System.Runtime.InteropServices.JavaScript;
        using System.Threading.Tasks;
        public static partial class JSObjectExtensions
        {
            public static Action? GetPropertyAsVoidActionNullable(this JSObject jsObject, string propertyName)
            {
                return jsObject.HasProperty(propertyName) ? MarshalAsVoidAction(jsObject, propertyName) : null;
            }
            [JSImport("unwrapProperty", "@typeshim")]
            [return: JSMarshalAs<JSType.Function>]
            public static partial Action MarshalAsVoidAction([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string propertyName);
        }
        
        """);
    }

    [TestCase("Action<int>", "Int32VoidAction", "JSType.Function<JSType.Number>")]
    [TestCase("Action<bool>", "BooleanVoidAction", "JSType.Function<JSType.Boolean>")]
    [TestCase("Action<string>", "StringVoidAction", "JSType.Function<JSType.String>")]
    [TestCase("Func<int>", "Int32Function", "JSType.Function<JSType.Number>")]
    [TestCase("Func<bool>", "BooleanFunction", "JSType.Function<JSType.Boolean>")]
    [TestCase("Func<string>", "StringFunction", "JSType.Function<JSType.String>")]
    [TestCase("Func<string, string>", "StringStringFunction", "JSType.Function<JSType.String, JSType.String>")]
    [TestCase("Func<string, bool, int>", "StringBooleanInt32Function", "JSType.Function<JSType.String, JSType.Boolean, JSType.Number>")]
    public void JSObjectExtensionsRendererTests_InstanceProperty_WithDelegateGenericType(string csTypeName, string managedSuffix, string jsType)
    {
        string source = """
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public {{type}} P1 { get; set; }
            }
        """.Replace("{{type}}", csTypeName);

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
            public static {{type}}? GetPropertyAs{{managed}}Nullable(this JSObject jsObject, string propertyName)
            {
                return jsObject.HasProperty(propertyName) ? MarshalAs{{managed}}(jsObject, propertyName) : null;
            }
            [JSImport("unwrapProperty", "@typeshim")]
            [return: JSMarshalAs<{{jstype}}>]
            public static partial {{type}} MarshalAs{{managed}}([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string propertyName);
        }
        
        """
        .Replace("{{type}}", csTypeName)
        .Replace("{{managed}}", managedSuffix)
        .Replace("{{jstype}}", jsType);

        AssertEx.EqualOrDiff(extensionsRenderContext.ToString(), expected);
    }

    [TestCase("Func<MyClass>", "Func<object>", "ObjectFunction", "JSType.Function<JSType.Any>")]
    [TestCase("Action<MyClass>", "Action<object>", "ObjectVoidAction", "JSType.Function<JSType.Any>")]
    [TestCase("Func<MyClass, MyClass>", "Func<object, object>", "ObjectObjectFunction", "JSType.Function<JSType.Any, JSType.Any>")]
    [TestCase("Func<MyClass, MyClass, MyClass>", "Func<object, object, object>", "ObjectObjectObjectFunction", "JSType.Function<JSType.Any, JSType.Any, JSType.Any>")]
    [TestCase("Action<MyClass, MyClass>", "Action<object, object>", "ObjectObjectVoidAction", "JSType.Function<JSType.Any, JSType.Any>")]
    [TestCase("Action<MyClass, MyClass, MyClass>", "Action<object, object, object>", "ObjectObjectObjectVoidAction", "JSType.Function<JSType.Any, JSType.Any, JSType.Any>")]
    [TestCase("Func<int, MyClass>", "Func<int, object>", "Int32ObjectFunction", "JSType.Function<JSType.Number, JSType.Any>")]
    [TestCase("Func<string, MyClass>", "Func<string, object>", "StringObjectFunction", "JSType.Function<JSType.String, JSType.Any>")]
    [TestCase("Func<bool, MyClass>", "Func<bool, object>", "BooleanObjectFunction", "JSType.Function<JSType.Boolean, JSType.Any>")]
    [TestCase("Func<long, MyClass>", "Func<long, object>", "Int64ObjectFunction", "JSType.Function<JSType.Number, JSType.Any>")]
    [TestCase("Func<MyClass, int>", "Func<object, int>", "ObjectInt32Function", "JSType.Function<JSType.Any, JSType.Number>")]
    [TestCase("Func<MyClass, string>", "Func<object, string>", "ObjectStringFunction", "JSType.Function<JSType.Any, JSType.String>")]
    [TestCase("Func<MyClass, bool>", "Func<object, bool>", "ObjectBooleanFunction", "JSType.Function<JSType.Any, JSType.Boolean>")]
    [TestCase("Func<MyClass, long>", "Func<object, long>", "ObjectInt64Function", "JSType.Function<JSType.Any, JSType.Number>")]
    [TestCase("Action<MyClass, int>", "Action<object, int>", "ObjectInt32VoidAction", "JSType.Function<JSType.Any, JSType.Number>")]
    [TestCase("Action<MyClass, string>", "Action<object, string>", "ObjectStringVoidAction", "JSType.Function<JSType.Any, JSType.String>")]
    [TestCase("Action<MyClass, bool>", "Action<object, bool>", "ObjectBooleanVoidAction", "JSType.Function<JSType.Any, JSType.Boolean>")]
    [TestCase("Action<MyClass, long>", "Action<object, long>", "ObjectInt64VoidAction", "JSType.Function<JSType.Any, JSType.Number>")]
    public void JSObjectExtensionsRendererTests_InstanceProperty_WithDelegateGenericType_IncludingUserClass(
        string exposedTypeName,
        string boundaryTypeName,
        string managedSuffix,
        string jsType)
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

        string source = """
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public {{type}} P1 { get; set; }
            }
        """.Replace("{{type}}", exposedTypeName);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(exportedClasses.Last(), typeCache).Build();

        List<InteropTypeInfo> types = [classInfo.Properties.First().Type];
        RenderContext extensionsRenderContext = new(classInfo, [classInfo, userClassInfo], RenderOptions.CSharp);
        new JSObjectExtensionsRenderer(extensionsRenderContext, types).Render();

        string expected = """    
        #nullable enable
        // JSImports for the type marshalling process
        using System;
        using System.Runtime.InteropServices.JavaScript;
        using System.Threading.Tasks;
        public static partial class JSObjectExtensions
        {
            public static {{type}}? GetPropertyAs{{managed}}Nullable(this JSObject jsObject, string propertyName)
            {
                return jsObject.HasProperty(propertyName) ? MarshalAs{{managed}}(jsObject, propertyName) : null;
            }
            [JSImport("unwrapProperty", "@typeshim")]
            [return: JSMarshalAs<{{jstype}}>]
            public static partial {{type}} MarshalAs{{managed}}([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string propertyName);
        }
        
        """
        .Replace("{{type}}", boundaryTypeName)
        .Replace("{{managed}}", managedSuffix)
        .Replace("{{jstype}}", jsType);

        AssertEx.EqualOrDiff(extensionsRenderContext.ToString(), expected);
    }
    
    [TestCase("Func<MyClass>", "Func<object>", "ObjectFunction", "JSType.Function<JSType.Any>")]
    [TestCase("Action<MyClass>", "Action<object>", "ObjectVoidAction", "JSType.Function<JSType.Any>")]
    [TestCase("Func<MyClass, MyClass>", "Func<object, object>", "ObjectObjectFunction", "JSType.Function<JSType.Any, JSType.Any>")]
    [TestCase("Func<MyClass, MyClass, MyClass>", "Func<object, object, object>", "ObjectObjectObjectFunction", "JSType.Function<JSType.Any, JSType.Any, JSType.Any>")]
    [TestCase("Action<MyClass, MyClass>", "Action<object, object>", "ObjectObjectVoidAction", "JSType.Function<JSType.Any, JSType.Any>")]
    [TestCase("Action<MyClass, MyClass, MyClass>", "Action<object, object, object>", "ObjectObjectObjectVoidAction", "JSType.Function<JSType.Any, JSType.Any, JSType.Any>")]
    [TestCase("Func<int, MyClass>", "Func<int, object>", "Int32ObjectFunction", "JSType.Function<JSType.Number, JSType.Any>")]
    [TestCase("Func<string, MyClass>", "Func<string, object>", "StringObjectFunction", "JSType.Function<JSType.String, JSType.Any>")]
    [TestCase("Func<bool, MyClass>", "Func<bool, object>", "BooleanObjectFunction", "JSType.Function<JSType.Boolean, JSType.Any>")]
    [TestCase("Func<long, MyClass>", "Func<long, object>", "Int64ObjectFunction", "JSType.Function<JSType.Number, JSType.Any>")]
    [TestCase("Func<MyClass, int>", "Func<object, int>", "ObjectInt32Function", "JSType.Function<JSType.Any, JSType.Number>")]
    [TestCase("Func<MyClass, string>", "Func<object, string>", "ObjectStringFunction", "JSType.Function<JSType.Any, JSType.String>")]
    [TestCase("Func<MyClass, bool>", "Func<object, bool>", "ObjectBooleanFunction", "JSType.Function<JSType.Any, JSType.Boolean>")]
    [TestCase("Func<MyClass, long>", "Func<object, long>", "ObjectInt64Function", "JSType.Function<JSType.Any, JSType.Number>")]
    [TestCase("Action<MyClass, int>", "Action<object, int>", "ObjectInt32VoidAction", "JSType.Function<JSType.Any, JSType.Number>")]
    [TestCase("Action<MyClass, string>", "Action<object, string>", "ObjectStringVoidAction", "JSType.Function<JSType.Any, JSType.String>")]
    [TestCase("Action<MyClass, bool>", "Action<object, bool>", "ObjectBooleanVoidAction", "JSType.Function<JSType.Any, JSType.Boolean>")]
    [TestCase("Action<MyClass, long>", "Action<object, long>", "ObjectInt64VoidAction", "JSType.Function<JSType.Any, JSType.Number>")]
    public void JSObjectExtensionsRendererTests_InstanceProperty_WithDelegateGenericType_IncludingUnexportedUserClass(
        string exposedTypeName,
        string boundaryTypeName,
        string managedSuffix,
        string jsType)
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            //[TSExport] not exported
            public class MyClass
            {
                public void M1()
                {
                }
            }
        """);

        string source = """
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public {{type}} P1 { get; set; }
            }
        """.Replace("{{type}}", exposedTypeName);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));

        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses.First(), typeCache).Build();

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
            public static {{type}}? GetPropertyAs{{managed}}Nullable(this JSObject jsObject, string propertyName)
            {
                return jsObject.HasProperty(propertyName) ? MarshalAs{{managed}}(jsObject, propertyName) : null;
            }
            [JSImport("unwrapProperty", "@typeshim")]
            [return: JSMarshalAs<{{jstype}}>]
            public static partial {{type}} MarshalAs{{managed}}([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string propertyName);
        }
        
        """
        .Replace("{{type}}", boundaryTypeName)
        .Replace("{{managed}}", managedSuffix)
        .Replace("{{jstype}}", jsType);

        AssertEx.EqualOrDiff(extensionsRenderContext.ToString(), expected);
    }
}
