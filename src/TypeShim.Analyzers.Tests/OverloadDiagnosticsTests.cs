using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class OverloadDiagnosticsTests
{
    private static readonly string OverloadId = TypeShimDiagnostics.NoOverloadsRule.Id;

    [Test]
    public async Task OverloadedMethods_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public void M(int x) { }
                public void M(string x) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, OverloadId);
    }

    [Test]
    public async Task OverloadedConstructors_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public C1(int x) { }
                public C1(string x) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, OverloadId);
    }

    [Test]
    public async Task ThreeOverloads_ReportsOnePerDuplicate()
    {
        // The seen-name set flags every occurrence beyond the first, so three overloads yield two diagnostics.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public void M(int x) { }
                public void M(string x) { }
                public void M(bool x) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertDiagnostics(diagnostics, OverloadId, OverloadId);
    }

    [Test]
    public async Task NonPublicOverloads_IsNotFlagged()
    {
        // Only public members participate in overload detection.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public void M(int x) { }
                private void M(string x) { }
                internal void M(bool x) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task DistinctMethodNames_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public void M(int x) { }
                public void N(string x) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }
}
