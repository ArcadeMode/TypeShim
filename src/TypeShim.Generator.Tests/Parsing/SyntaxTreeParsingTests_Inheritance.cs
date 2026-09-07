using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Shared;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests.Parsing;

internal class SyntaxTreeParsingTests_Inheritance
{
    private static INamedTypeSymbol GetExportedClass(string source, string name)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exported = [.. symbolExtractor.ExtractAllExportedSymbols()];
        return exported.Single(s => s.Name == name);
    }

    [Test]
    public void ClassInfoBuilder_Throws_ForTSExportBaseClass()
    {
        INamedTypeSymbol classSymbol = GetExportedClass("""
            using System;
            namespace N1;
            [TSExport]
            public class BaseC { public int P { get; set; } }
            [TSExport]
            public class C1 : BaseC { }
        """, "C1");

        Assert.Throws<NotSupportedInheritanceException>(() =>
            _ = new ClassInfoBuilder(classSymbol, new InteropTypeInfoCache()).Build());
    }

    [Test]
    public void ClassInfoBuilder_Throws_ForNonExportedBaseClass()
    {
        // The base is stripped from the partial compilation and becomes an error type, but its
        // presence (non-object base) is still detectable.
        INamedTypeSymbol classSymbol = GetExportedClass("""
            using System;
            namespace N1;
            public class BaseC { public int P { get; set; } }
            [TSExport]
            public class C1 : BaseC { }
        """, "C1");

        Assert.Throws<NotSupportedInheritanceException>(() =>
            _ = new ClassInfoBuilder(classSymbol, new InteropTypeInfoCache()).Build());
    }

    [Test]
    public void ClassInfoBuilder_Throws_ForNonDisposableInterface()
    {
        INamedTypeSymbol classSymbol = GetExportedClass("""
            using System;
            namespace N1;
            public interface IThing { void Do(); }
            [TSExport]
            public class C1 : IThing { public void Do() { } }
        """, "C1");

        Assert.Throws<NotSupportedInheritanceException>(() =>
            _ = new ClassInfoBuilder(classSymbol, new InteropTypeInfoCache()).Build());
    }

    [Test]
    public void ClassInfoBuilder_DoesNotThrow_ForDisposableOnly()
    {
        INamedTypeSymbol classSymbol = GetExportedClass("""
            using System;
            namespace N1;
            [TSExport]
            public class C1 : IDisposable { public int P { get; set; } public void Dispose() { } }
        """, "C1");

        Assert.DoesNotThrow(() =>
            _ = new ClassInfoBuilder(classSymbol, new InteropTypeInfoCache()).Build());
    }

    [Test]
    public void ClassInfoBuilder_DoesNotThrow_ForPlainClass()
    {
        INamedTypeSymbol classSymbol = GetExportedClass("""
            using System;
            namespace N1;
            [TSExport]
            public class C1 { public int P { get; set; } }
        """, "C1");

        Assert.DoesNotThrow(() =>
            _ = new ClassInfoBuilder(classSymbol, new InteropTypeInfoCache()).Build());
    }
}
