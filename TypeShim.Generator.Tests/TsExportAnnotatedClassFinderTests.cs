using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests;

public class TSExportAnnotatedClassFinderTests
{
    [Test]
    public void ClassSyntax_WithoutTsExportAttribute_DoesNot_GetProcessed()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(@"
                using System;
                public class SampleClass
                {
                    public static async Task<object> SampleMethod(object param1, int param2)
                    {
                        return await Task.FromResult(param1);
                    }
                }
            ");

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        IEnumerable<INamedTypeSymbol> exportedClasses = TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot());
        Assert.That(exportedClasses, Is.Empty);
    }

    [Test]
    public void ClassSyntax_WithTsExportAttribute_Does_GetProcessed()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(@"
            using System;

            [TSExport]
            public class SampleClass
            {
                public static async Task<object> SampleMethod(object param1, int param2)
                {
                    return await Task.FromResult(param1);
                }
            }
        ");

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
    }

    [Test]
    public void ClassSyntax_WithTsExportAttribute_Do_GetProcessed_Multiple()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(@"
            using System;
            [TSExport]
            public class SampleClass
            {
                public static async Task<object> SampleMethod(object param1, int param2)
                {
                    return await Task.FromResult(param1);
                }
            }
            [TSExportAttribute]
            public class SampleClass2
            {
                public static async Task<object> SampleMethod(object param1, int param2)
                {
                    return await Task.FromResult(param1);
                }
            }
        ");

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TSExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
    }
}
