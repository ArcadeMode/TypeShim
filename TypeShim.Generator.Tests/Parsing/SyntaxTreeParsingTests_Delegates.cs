using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.Parsing;

internal class SyntaxTreeParsingTests_Delegates
{
    [TestCase("int[]")]
    [TestCase("int[]?")]
    [TestCase("int?")]
    [TestCase("char?")]
    public void ClassInfoBuilder_RejectsUnsupportedInnerTypes(string innerType)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                public void M1(Func<{{innerType}}> func)
                {
                }
            }
        """.Replace("{{innerType}}", innerType));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        InteropTypeInfoCache typeCache = new();
        Assert.Throws<NotSupportedTypeException>(() => new ClassInfoBuilder(classSymbol, typeCache).Build());
    }

    
}

