using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests;

internal class SyntaxTreeParsingTests_StaticProperties
{
    [Test]
    public void ClassInfoBuilder_ParsesStaticProperties()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TsExport]
            public class C1
            {
                public static int P1 { get; set; }
            }
        """);

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        Assert.That(classInfo.Properties.ToList(), Has.Count.EqualTo(1));
        PropertyInfo propertyInfo = classInfo.Properties.First();
        Assert.That(propertyInfo.Name, Is.EqualTo("P1"));
        Assert.That(propertyInfo.GetMethod.IsStatic, Is.True);
        Assert.That(propertyInfo.SetMethod?.IsStatic, Is.True);

    }
}
