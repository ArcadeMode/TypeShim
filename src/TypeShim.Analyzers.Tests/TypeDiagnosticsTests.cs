using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class TypeDiagnosticsTests
{
    private static readonly string UnsupportedTypeId = TypeShimDiagnostics.UnsupportedTypeRule.Id;
    private static readonly string NonExportedTypeId = TypeShimDiagnostics.NonExportedTypeInInteropApiRule.Id;

    // --- TSHIM005 UnsupportedType (non-enum path) ---

    [Test]
    public async Task UnsupportedReturnType_IsFlaggedAsUnsupported()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public decimal M() => 0m;
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, UnsupportedTypeId);
    }

    [Test]
    public async Task UnsupportedParameterType_IsFlaggedAsUnsupported()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public void M(decimal value) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, UnsupportedTypeId);
    }

    [Test]
    public async Task UnsupportedPropertyType_IsFlaggedAsUnsupported()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public decimal P { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, UnsupportedTypeId);
    }

    // --- TSHIM006 NonExportedTypeInInteropApi (non-enum path) ---

    [Test]
    public async Task NonExportedReturnType_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            public class Other { }

            [TSExport]
            public class C1
            {
                public Other M() => new Other();
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, NonExportedTypeId);
    }

    [Test]
    public async Task NonExportedParameterType_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            public class Other { }

            [TSExport]
            public class C1
            {
                public void M(Other other) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, NonExportedTypeId);
    }

    [Test]
    public async Task NonExportedPropertyType_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            public class Other { }

            [TSExport]
            public class C1
            {
                public Other P { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, NonExportedTypeId);
    }

    [Test]
    public async Task TSExportReferencedType_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class Other { }

            [TSExport]
            public class C1
            {
                public Other M() => new Other();
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }
}
