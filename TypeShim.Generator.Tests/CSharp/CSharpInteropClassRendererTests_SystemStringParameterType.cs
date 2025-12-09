using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_SystemStringParameterType
{
    [Test]
    public void CSharpInteropClass_StaticMethod_HasJSTypeString_ForStringParameterType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TsExport]
            public class C1
            {
                public static void M1(string p1)
                {
                    bool b = string.IsNullOrEmpty(p1);
                }
            }
        """);

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
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.String>] string p1)
    {
        C1.M1(p1);
    }
}

"""));
    }

    [Test]
    public void CSharpInteropClass_DynamicMethod_HasJSTypeString_ForStringParameterType()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TsExport]
            public class C1
            {
                public void M1(string p1)
                {
                    bool b = string.IsNullOrEmpty(p1);
                }
            }
        """);

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
    [return: JSMarshalAs<JSType.Void>]
    public static void M1([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.String>] string p1)
    {
        C1 typed_instance = (C1)instance;
        typed_instance.M1(p1);
    }
}

"""));
    }
}
