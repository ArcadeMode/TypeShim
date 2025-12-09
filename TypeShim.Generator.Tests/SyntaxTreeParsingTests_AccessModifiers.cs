using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests;

internal class SyntaxTreeParsingTests_AccessModifiers
{
    [Test]
    public void ClassInfoBuilder_DoesNotParseNonPublicMethods()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                internal static int M1()
                {
                    return 1;
                }
                internal int M2()
                {
                    return 1;
                }
                protected static int M2()
                {
                    return 1;
                }
                protected int M2()
                {
                    return 1;
                }
                private static int M2()
                {
                    return 1;
                }
                private int M2()
                {
                    return 1;
                }
            }
        """);

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        Assert.That(classInfo.Methods.ToList(), Has.Count.EqualTo(0));
    }

    [Test]
    public void ClassInfoBuilder_DoesNotParseNonPublicProperties()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                internal static int P1 { get; set; }
                internal int P2 { get; set; }
                protected static int P3 { get; set; }
                protected int P4 { get; set; }
                private static int P5 { get; set; }
                private int P6 { get; set; }
            }
        """);

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        Assert.That(classInfo.Properties.ToList(), Has.Count.EqualTo(0));
    }

    [Test]
    public void ClassInfoBuilder_DoesNotParseNonPublicPropertySetters()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                public static int P1 { get; private set; }
                public int P2 { get; private set; }
            }
        """);

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        Assert.That(classInfo.Properties.ToList(), Has.Count.EqualTo(2));
        Assert.That(classInfo.Properties.All(p => p.SetMethod is null), Is.True); 
    }
}
