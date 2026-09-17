using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class NestedExportAnnotationDiagnosticsTests
{
    private static readonly string NestedWithoutContainerId = TypeShimDiagnostics.NestedTSExportWithoutExportedContainerRule.Id;
    private static readonly string RedundantNestedId = TypeShimDiagnostics.RedundantNestedTSExportRule.Id;

    [Test]
    public async Task NestedTSExport_WithoutExportedContainer_IsWarned()
    {
        string source = """
            using TypeShim;

            public class Outer
            {
                [TSExport]
                public class Inner
                {
                    public int Value { get; set; }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, NestedWithoutContainerId);
    }

    [Test]
    public async Task NestedTSExport_WithExportedContainer_IsRedundant()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class Outer
            {
                [TSExport]
                public class Inner
                {
                    public int Value { get; set; }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, RedundantNestedId);
    }

    [Test]
    public async Task NestedTSExportEnum_WithExportedContainer_IsRedundant()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class Outer
            {
                [TSExport]
                public enum Kind { A, B }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, RedundantNestedId);
    }

    [Test]
    public async Task NestedTSExport_UnderNonExportedTopLevel_TreatsAncestorChainAsNotExported()
    {
        // Middle has no [TSExport] ancestor -> warning; Inner inherits exportedness from Middle -> redundant.
        string source = """
            using TypeShim;

            public class Outer
            {
                [TSExport]
                public class Middle
                {
                    [TSExport]
                    public class Inner
                    {
                        public int Value { get; set; }
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertDiagnostics(diagnostics, NestedWithoutContainerId, RedundantNestedId);
    }
}
