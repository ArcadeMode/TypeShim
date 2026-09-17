using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class AccessibilityDiagnosticsTests
{
    private static readonly string PublicOnlyId = TypeShimDiagnostics.AttributeOnPublicClassOnlyRule.Id;
    private static readonly string OverloadId = TypeShimDiagnostics.NoOverloadsRule.Id;

    [Test]
    public async Task InternalTSExportClass_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            internal class C1
            {
                public int P { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, PublicOnlyId);
    }

    [Test]
    public async Task FileScopedTSExportClass_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            file class C1
            {
                public int P { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, PublicOnlyId);
    }

    [Test]
    public async Task PrivateNestedTSExportClass_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            public class Outer
            {
                [TSExport]
                private class C1
                {
                    public int P { get; set; }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, PublicOnlyId);
    }

    [Test]
    public async Task PublicTSExportClass_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public int P { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task NonPublicClassWithoutTSExport_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            internal class C1
            {
                public int P { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task InternalTSExportClass_WithOverload_ReportsBothDiagnostics()
    {
        // Accessibility is reported alongside member checks: AnalyzeClass continues after the
        // accessibility check, so a non-public class with an overloaded member yields both diagnostics.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            internal class C1
            {
                public void M(int x) { }
                public void M(string x) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertDiagnostics(diagnostics, PublicOnlyId, OverloadId);
    }


    [Test]
    public async Task PublicNestedType_UnderExportedContainer_IsNotFlagged()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class Outer
            {
                public int Id { get; set; }

                public class Inner
                {
                    public int Value { get; set; }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task PrivateNestedType_UnderExportedContainer_IsNotAnalyzed()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class Outer
            {
                public int Id { get; set; }

                private class Hidden
                {
                    public decimal M() => 0m;
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }
}
