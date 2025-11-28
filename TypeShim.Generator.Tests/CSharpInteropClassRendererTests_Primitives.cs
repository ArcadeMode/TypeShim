using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests;

internal class CSharpInteropClassRendererTests_Primitives
{
    [TestCase("int16")]
    [TestCase("int32")]
    [TestCase("int")]
    [TestCase("int64")]
    [TestCase("long")]
    [TestCase("double")]
    [TestCase("IntPtr")]
    public void CSharpInteropClass_Number_JSType(string typeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            namespace M1;
            [TsExport]
            public class C1
            {
                [JSMarshalAs(JSType.Number)]
                public static {{typeExpression}} M1()
                {
                    return 1;
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));

        INamedTypeSymbol classSymbol = exportedClasses[0];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        string interopClass = new CSharpInteropClassRenderer(classInfo).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop definitions
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace M1;
public partial class C1Interop
{
    [JSExport]
    public static {{typeExpression}} M1()
    {
        return C1.M1();
    }
}

""".Replace("{{typeExpression}}", typeExpression)));
    }
}
