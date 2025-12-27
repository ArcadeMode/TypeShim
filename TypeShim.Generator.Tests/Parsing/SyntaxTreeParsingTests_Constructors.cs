using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.Parsing;

internal class SyntaxTreeParsingTests_Constructors
{
    [Test]
    public void ClassInfoBuilder_ParsesPrimaryConstructor()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1(string name, int value)
            {
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        Assert.That(classInfo.Constructor, Is.Not.Null);
        Assert.That(classInfo.Constructor.MethodParameters.ToList(), Has.Count.EqualTo(2));
    }

    [Test]
    public void ClassInfoBuilder_ParsesRegularConstructor()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                public C1(string name, int value)
                {
                }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        Assert.That(classInfo.Constructor, Is.Not.Null);
        Assert.That(classInfo.Constructor.MethodParameters.ToList(), Has.Count.EqualTo(2));
    }

    [Test]
    public void ClassInfoBuilder_RejectsConstructorOverloads()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                public C1(string name, int value)
                {
                }

                public C1(string name, int value, bool flag) : this(name, value)
                {
                }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        Assert.Throws<UnsupportedConstructorOverloadException>(() => new ClassInfoBuilder(classSymbol).Build());
    }

    [Test]
    public void ClassInfoBuilder_RejectsPrimaryConstructorOverloads()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1(string name, int value)
            {
                public C1(string name, int value, bool flag) : this(name, value)
                {
                }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        Assert.Throws<UnsupportedConstructorOverloadException>(() => new ClassInfoBuilder(classSymbol).Build());
    }
}
