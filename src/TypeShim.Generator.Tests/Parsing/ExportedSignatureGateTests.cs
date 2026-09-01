using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.Parsing;

internal class ExportedSignatureGateTests
{
    private static SymbolExtractor Extractor(string source)
        => new([CSharpFileInfo.Create(CSharpSyntaxTree.ParseText(source))], TestFixture.TargetingPackRefDir);

    [Test]
    public void ExtractAllExportedSymbols_InstanceMemberInStaticClass_ThrowsWithCs0708()
    {
        // CS0708: instance member declared in a static class. Extracts into a valid-looking symbol
        // but guarantees broken codegen, so the gate must stop the run.
        SymbolExtractor extractor = Extractor("""
            using System;
            namespace N1;
            [TSExport]
            public static class C1
            {
                public string M1()
                {
                    return "x";
                }
            }
        """);

        InvalidCodeException ex = Assert.Throws<InvalidCodeException>(() => extractor.ExtractAllExportedSymbols());
        Assert.That(ex!.Message, Does.Contain("CS0708"));
    }

    [Test]
    public void ExtractAllExportedSymbols_StrayBraceInBody_ThrowsSyntaxError()
    {
        // A stray brace reparents sibling members out of the class; the syntax gate must catch it
        // even though the signature span itself is clean.
        SymbolExtractor extractor = Extractor("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                private C1() {}
                public string M1() { return "x"; }}
                public string M2() { return "y"; }
            }
        """);

        InvalidCodeException ex = Assert.Throws<InvalidCodeException>(() => extractor.ExtractAllExportedSymbols());
        Assert.That(ex!.Message, Does.Contain("invalid code"));
    }

    [Test]
    public void ExtractAllExportedSymbols_ValidSignatureReferencingNonExportedType_GateDoesNotThrow()
    {
        // The parameter type 'Foo' is valid C# but non-exported, so the rewriter strips it and the
        // signature span reports CS0246. That is a partial-compilation artifact the gate must ignore;
        // rejection (if any) belongs to the parser downstream, not this gate.
        SymbolExtractor extractor = Extractor("""
            using System;
            namespace N1;
            public class Foo {}
            [TSExport]
            public class C1
            {
                private C1() {}
                public void M1(Foo f) {}
            }
        """);

        List<INamedTypeSymbol> exported = [.. extractor.ExtractAllExportedSymbols()];
        Assert.That(exported.Select(s => s.Name), Does.Contain("C1"));
    }

    [Test]
    public void ExtractAllExportedSymbols_RichValidSurface_DoesNotThrow()
    {
        SymbolExtractor extractor = Extractor("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                private C1() {}
                public DateTime P1 { get; set; }
                public Task<int> M1(string s, int[] arr, DateTime dt, Func<int, int> f, bool? b)
                {
                    return Task.FromResult(1);
                }
                public void M2(TimeSpan ts, double d, long l, char c) {}
            }
        """);

        List<INamedTypeSymbol> exported = [.. extractor.ExtractAllExportedSymbols()];
        Assert.That(exported.Select(s => s.Name), Does.Contain("C1"));
    }

    [Test]
    public void ExtractAllExportedSymbols_CustomAttributeOnMethod_DoesNotThrow()
    {
        // The custom attribute is not in the partial compilation's references, so anchoring the
        // signature span at the node start would surface a spurious CS0246. Anchoring at the return
        // type excludes attributes, so the gate must not throw here.
        SymbolExtractor extractor = Extractor("""
            using System;
            namespace N1;
            public sealed class MyCustomAttribute : Attribute {}
            [TSExport]
            public class C1
            {
                private C1() {}
                [MyCustom]
                public string M1()
                {
                    return "x";
                }
            }
        """);

        List<INamedTypeSymbol> exported = [.. extractor.ExtractAllExportedSymbols()];
        Assert.That(exported.Select(s => s.Name), Does.Contain("C1"));
    }
}
