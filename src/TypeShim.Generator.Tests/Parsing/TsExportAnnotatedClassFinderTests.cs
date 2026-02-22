using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests.Parsing;

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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        Assert.That(symbolExtractor.ExtractAllExportedSymbols(), Is.Empty);
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
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
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

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
    }
}
