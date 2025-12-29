using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.Parsing;

internal class SyntaxTreeParsingTests_Properties
{
    [Test]
    public void ClassInfoBuilder_PublicStaticProperty_Parses()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                public static int P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        Assert.That(classInfo.Properties.ToList(), Has.Count.EqualTo(1));
        PropertyInfo propertyInfo = classInfo.Properties.First();
        Assert.That(propertyInfo.Name, Is.EqualTo("P1"));
        Assert.That(propertyInfo.GetMethod.IsStatic, Is.True);
        Assert.That(propertyInfo.SetMethod?.IsStatic, Is.True);

    }

    [Test]
    public void ClassInfoBuilder_NonPublicStaticProperty_IsIgnored()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                internal static int P1 { get; set; }
                protected static int P2 { get; set; }
                private static int P3 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeCache).Build();
        Assert.That(classInfo.Properties.ToList(), Has.Count.EqualTo(0));
    }

    [Test]
    public void ClassInfoBuilder_NonPublicRequiredMemberProperty_Throws()
    {
        // this is not valid C# syntax anyway, but if provided this kind of input, no invalid code should be generated.
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                internal required int P1 { get; set; }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)]);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        InteropTypeInfoCache typeCache = new();
        Assert.Throws<NotSupportedPropertyException>(() => new ClassInfoBuilder(classSymbol, typeCache).Build());
    }
}
