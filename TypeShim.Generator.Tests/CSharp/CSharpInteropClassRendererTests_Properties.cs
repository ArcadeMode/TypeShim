using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_Properties
{
    [TestCase("string", "string", "JSType.String")]
    [TestCase("double", "double", "JSType.Number")]
    [TestCase("bool", "bool", "JSType.Boolean")]
    public void CSharpInteropClass_InstanceProperty_GeneratesGetAndSetFunctions(string typeExpression, string interopTypeExpression, string jsType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class C1
            {
                public {{typeExpression}} P1 { get; set; }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        string interopClass = new CSharpInteropClassRenderer(classInfo).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<{{jsType}}>]
    public static {{typeExpression}} get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<{{jsType}}>] {{typeExpression}} value)
    {
        C1 typed_instance = (C1)instance;
        typed_instance.P1 = value;
    }
}

""".Replace("{{typeExpression}}", interopTypeExpression).Replace("{{jsType}}", jsType)));
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithUserClassType()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
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
            [TsExport]
            public class C1
            {
                public MyClass P1 { get; set; }
            }
        """);
        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([userClass, syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.Last();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        string interopClass = new CSharpInteropClassRenderer(classInfo).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
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
        MyClass typed_value = (MyClass)value;
        typed_instance.P1 = typed_value;
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_InstanceProperty_WithNullableUserClassType()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
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
            [TsExport]
            public class C1
            {
                public MyClass? P1 { get; set; }
            }
        """);
        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([userClass, syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.Last();

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        string interopClass = new CSharpInteropClassRenderer(classInfo).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace N1;
public partial class C1Interop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object? get_P1([JSMarshalAs<JSType.Any>] object instance)
    {
        C1 typed_instance = (C1)instance;
        return typed_instance.P1;
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void set_P1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object? value)
    {
        C1 typed_instance = (C1)instance;
        MyClass? typed_value = (MyClass?)value;
        typed_instance.P1 = typed_value;
    }
}

"""));
    }
}
