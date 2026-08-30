using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.Parsing;

internal class EnumDiscoveryTests
{
    private static List<INamedTypeSymbol> Extract(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        return [.. symbolExtractor.ExtractAllExportedSymbols()];
    }

    private static EnumInfo BuildEnum(string source, string name = "Color")
    {
        INamedTypeSymbol enumSymbol = Extract(source).First(s => s.Name == name);
        return new EnumInfoBuilder(enumSymbol, new InteropTypeInfoCache()).Build();
    }

    [Test]
    public void EnumDiscovery_KeepsTSExportEnum_DropsNonTSExportEnum()
    {
        List<INamedTypeSymbol> symbols = Extract("""
            namespace N1;
            [TSExport]
            public enum Kept { A, B }
            public enum Dropped { C, D }
        """);

        List<string> enumNames = [.. symbols.Where(s => s.TypeKind == TypeKind.Enum).Select(s => s.Name)];

        Assert.That(enumNames, Is.EqualTo(new[] { "Kept" }));
    }

    [Test]
    public void EnumInfoBuilder_Throws_ForUnexportedEnumReferencedByExportedClass()
    {
        // A non-exported enum referenced on the boundary is dropped from the export-only compilation,
        // so it resolves to an error type and building the referencing class throws - symmetric with unexported classes.
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            namespace N1;
            public enum Color { Red, Green, Blue }
            [TSExport]
            public class C1
            {
                public Color P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedSymbols = [.. symbolExtractor.ExtractAllExportedSymbols()];
        INamedTypeSymbol classSymbol = exportedSymbols.First(s => s.Name == "C1");

        Assert.Throws<NotSupportedTypeException>(() =>
            _ = new ClassInfoBuilder(classSymbol, new InteropTypeInfoCache()).Build());
    }

    [Test]
    public void EnumInfoBuilder_ReadsMembersInOrderWithExactValues()
    {
        EnumInfo enumInfo = BuildEnum("""
            namespace N1;
            [TSExport]
            public enum Color { Red, Green = 5, Blue }
        """);

        Assert.Multiple(() =>
        {
            Assert.That(enumInfo.Name, Is.EqualTo("Color"));
            Assert.That(enumInfo.Namespace, Is.EqualTo("N1"));
            Assert.That(enumInfo.UnderlyingType, Is.EqualTo("int"));
            Assert.That(enumInfo.Members.Select(m => m.Name), Is.EqualTo(new[] { "Red", "Green", "Blue" }));
            Assert.That(enumInfo.Members.Select(m => m.Value), Is.EqualTo(new long[] { 0, 5, 6 }));
        });
    }

    [Test]
    public void EnumInfoBuilder_FoldsFlagCombinationValues()
    {
        EnumInfo enumInfo = BuildEnum("""
            using System;
            namespace N1;
            [Flags]
            [TSExport]
            public enum Color { None = 0, A = 1, B = 2, AB = A | B }
        """);

        Assert.That(enumInfo.Members.Select(m => m.Value), Is.EqualTo(new long[] { 0, 1, 2, 3 }));
    }

    [Test]
    public void EnumInfoBuilder_ReflectsUIntUnderlyingType()
    {
        EnumInfo enumInfo = BuildEnum("""
            namespace N1;
            [TSExport]
            public enum Color : uint { Red, Green, Blue }
        """);

        Assert.That(enumInfo.UnderlyingType, Is.EqualTo("uint"));
    }

    [TestCase("long")]
    [TestCase("ulong")]
    public void EnumInfoBuilder_UnsupportedUnderlyingType_Throws(string underlying)
    {
        Assert.Throws<NotSupportedTypeException>(() => BuildEnum("""
            namespace N1;
            [TSExport]
            public enum Color : {{underlying}} { Red, Green, Blue }
        """.Replace("{{underlying}}", underlying)));
    }

    [Test]
    public void SymbolMap_GetNamedTypeInfo_ResolvesEnum_ButGetClassInfoThrows()
    {
        EnumInfo enumInfo = BuildEnum("""
            namespace N1;
            [TSExport]
            public enum Color { Red, Green, Blue }
        """);

        SymbolMap symbolMap = new([enumInfo]);

        Assert.Multiple(() =>
        {
            Assert.That(symbolMap.GetNamedTypeInfo(enumInfo.Type), Is.SameAs(enumInfo));
            Assert.Throws<NotFoundClassInfoException>(() => symbolMap.GetClassInfo(enumInfo.Type));
        });
    }
}
