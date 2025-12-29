using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.Parsing;

internal class SyntaxTreeParsingTests_Methods
{
    [Test]
    public void ClassInfoBuilder_RejectsMethodOverloads()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
        using System;
        namespace N1;
        [TSExport]
        public class C1
        {
            public void M1(string name, int value)
            {
            }

            public void M1(string name, int value, bool flag)
            {
            }
        }
    """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        Assert.Throws<NotSupportedMethodOverloadException>(() => new ClassInfoBuilder(classSymbol).Build());
    }

    [Test]
    public void ClassInfoBuilder_ParsesPublicMethodOverloads_IfOnlyOneIsPublic()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
        using System;
        namespace N1;
        [TSExport]
        public class C1
        {
            public void M1(string name, int value)
            {
            }

            internal void M1(string name, int value, bool flag)
            {
            }
        }
    """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        Assert.That(classInfo.Methods.ToList(), Has.Count.EqualTo(1));
    }
}

