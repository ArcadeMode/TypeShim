using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class RecordDiagnosticsTests
{
    private static readonly string RecordNotSupportedId = TypeShimDiagnostics.RecordNotSupportedRule.Id;

    [Test]
    public async Task TSExportRecord_IsFlaggedAsUnsupported()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public record class C1
            {
                public string Name { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, RecordNotSupportedId);
    }

    [Test]
    public async Task TSExportPositionalRecord_IsFlaggedAsUnsupported()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public record C1(string Name, int Value);
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, RecordNotSupportedId);
    }

    [Test]
    public async Task TSExportClass_IsNotFlaggedAsRecord()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public string Name { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }
}
