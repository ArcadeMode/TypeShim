using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.Parsing;

internal class SyntaxTreeParsingTests_Enums
{
    private static INamedTypeSymbol GetSymbol(string source, string name)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedSymbols = [.. symbolExtractor.ExtractAllExportedSymbols()];
        return exportedSymbols.First(s => s.Name == name);
    }

    [Test]
    public void InteropTypeInfoBuilder_TSExportEnum_IsClassifiedAsConvertibleNumber()
    {
        INamedTypeSymbol enumSymbol = GetSymbol("""
            namespace N1;
            [TSExport]
            public enum Color { Red, Green, Blue }
        """, "Color");

        InteropTypeInfo info = new InteropTypeInfoBuilder(enumSymbol, new InteropTypeInfoCache()).Build();

        Assert.Multiple(() =>
        {
            Assert.That(info.IsEnum, Is.True);
            Assert.That(info.ManagedType, Is.EqualTo(KnownManagedType.Int32));
            Assert.That(info.RequiresTypeConversion, Is.True);
            Assert.That(info.SupportsTypeConversion, Is.True);
            Assert.That(info.CSharpTypeSyntax.ToString(), Is.EqualTo("Color"));
            Assert.That(info.CSharpInteropTypeSyntax.ToString(), Is.EqualTo("int"));
            Assert.That(info.JSTypeSyntax.ToString(), Is.EqualTo("JSType.Number"));
        });
    }

    [TestCase("byte")]
    [TestCase("sbyte")]
    [TestCase("short")]
    [TestCase("ushort")]
    [TestCase("int")]
    public void InteropTypeInfoBuilder_SafeSmallUnderlyingType_MarshalsAsInt(string underlying)
    {
        INamedTypeSymbol enumSymbol = GetSymbol("""
            namespace N1;
            [TSExport]
            public enum Color : {{underlying}} { Red, Green, Blue }
        """.Replace("{{underlying}}", underlying), "Color");

        InteropTypeInfo info = new InteropTypeInfoBuilder(enumSymbol, new InteropTypeInfoCache()).Build();

        Assert.Multiple(() =>
        {
            Assert.That(info.ManagedType, Is.EqualTo(KnownManagedType.Int32));
            Assert.That(info.CSharpInteropTypeSyntax.ToString(), Is.EqualTo("int"));
        });
    }

    [Test]
    public void InteropTypeInfoBuilder_UIntUnderlyingType_MarshalsAsLong()
    {
        INamedTypeSymbol enumSymbol = GetSymbol("""
            namespace N1;
            [TSExport]
            public enum Color : uint { Red, Green, Blue }
        """, "Color");

        InteropTypeInfo info = new InteropTypeInfoBuilder(enumSymbol, new InteropTypeInfoCache()).Build();

        Assert.Multiple(() =>
        {
            Assert.That(info.ManagedType, Is.EqualTo(KnownManagedType.Int64));
            Assert.That(info.CSharpInteropTypeSyntax.ToString(), Is.EqualTo("long"));
        });
    }

    [TestCase("long")]
    [TestCase("ulong")]
    public void InteropTypeInfoBuilder_UnsafeUnderlyingType_Throws(string underlying)
    {
        INamedTypeSymbol enumSymbol = GetSymbol("""
            namespace N1;
            [TSExport]
            public enum Color : {{underlying}} { Red, Green, Blue }
        """.Replace("{{underlying}}", underlying), "Color");

        Assert.Throws<NotSupportedTypeException>(() =>
            _ = new InteropTypeInfoBuilder(enumSymbol, new InteropTypeInfoCache()).Build());
    }

    [Test]
    public void ClassInfoBuilder_TSExportEnumProperty_IsConvertible()
    {
        INamedTypeSymbol classSymbol = GetSymbol("""
            namespace N1;
            [TSExport]
            public enum Color { Red, Green, Blue }
            [TSExport]
            public class C1
            {
                public Color P1 { get; set; }
            }
        """, "C1");

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, new InteropTypeInfoCache()).Build();
        PropertyInfo propertyInfo = classInfo.Properties.First(p => p.Name == "P1");

        Assert.Multiple(() =>
        {
            Assert.That(propertyInfo.Type.IsEnum, Is.True);
            Assert.That(propertyInfo.Type.RequiresTypeConversion, Is.True);
            Assert.That(propertyInfo.Type.SupportsTypeConversion, Is.True);
        });
    }
}
