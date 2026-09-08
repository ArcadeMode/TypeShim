using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class OptionalParameterDiagnosticsTests
{
    private static readonly string ConstScopeId = TypeShimDiagnostics.UnresolvableDefaultConstRule.Id;
    private static readonly string OptionalMemoryViewId = TypeShimDiagnostics.NoOptionalMemoryViewRule.Id;

    [Test]
    public async Task CrossClassConstDefault_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            public static class Defaults { public const int Timeout = 30; }

            [TSExport]
            public class C1
            {
                public void M(int timeout = Defaults.Timeout) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, ConstScopeId);
    }

    [Test]
    public async Task SameClassConstDefault_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public const int Timeout = 30;
                public void M(int timeout = Timeout) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task ConstInAnotherTSExportClass_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class Other { public const int Timeout = 30; }

            [TSExport]
            public class C1
            {
                public void M(int timeout = Other.Timeout) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task FrameworkConstDefault_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public void M(int value = int.MaxValue) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task LiteralDefault_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public void M(int count = 5, string label = "abc") { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task SpanDefault_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public void M(Span<int> data = default) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, OptionalMemoryViewId);
    }

    [Test]
    public async Task ArraySegmentDefault_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public void M(ArraySegment<int> data = default) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, OptionalMemoryViewId);
    }

    [Test]
    public async Task RequiredSpanParameter_IsNotFlaggedAsOptional()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public void M(Span<int> data) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }
}
