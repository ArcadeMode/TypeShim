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
        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        Assert.That(classInfo.Constructor, Is.Not.Null);
        Assert.That(classInfo.Constructor.Parameters, Has.Length.EqualTo(2));
    }

    [Test]
    public void ClassInfoBuilder_RegularConstructor_Parses()
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
        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        Assert.That(classInfo.Constructor, Is.Not.Null);
        Assert.That(classInfo.Constructor.Parameters, Has.Length.EqualTo(2));
    }

    [Test]
    public void ClassInfoBuilder_NonPublicConstructorOverload_Parses()
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

                internal C1(string name, int value, bool internalP) : this(name, value)
                {
                }

                protected C1(string name, int value, int protectedP) : this(name, value)
                {
                }

                private C1(string name, int value, bool privateP) : this(name, value)
                {
                }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        Assert.That(classInfo.Constructor, Is.Not.Null);
        Assert.That(classInfo.Constructor.Parameters, Has.Length.EqualTo(2)); // only public has 2
    }

    [Test]
    public void ClassInfoBuilder_ConstructorOverload_ThrowsNotSupported()
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
        InteropTypeInfoCache typeCache = new();
        Assert.Throws<NotSupportedConstructorOverloadException>(() => new ClassInfoBuilder(classSymbol, typeCache).Build());
    }

    [Test]
    public void ClassInfoBuilder_PrimaryConstructorOverload_ThrowsNotSupported()
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
        InteropTypeInfoCache typeCache = new();
        Assert.Throws<NotSupportedConstructorOverloadException>(() => new ClassInfoBuilder(classSymbol, typeCache).Build());
    }
}
