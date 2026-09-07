using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class InheritanceDiagnosticsTests
{
    private static readonly string UnsupportedInheritanceId = TypeShimDiagnostics.UnsupportedInheritanceRule.Id;

    [Test]
    public async Task ConcreteBaseClass_IsFlagged()
    {
        string source = """
            using TypeShim;

            public class BaseC { public int P { get; set; } }

            [TSExport]
            public class C1 : BaseC { }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, UnsupportedInheritanceId);
    }

    [Test]
    public async Task AbstractBaseClass_IsFlagged()
    {
        string source = """
            using TypeShim;

            public abstract class BaseC { public abstract int P { get; set; } }

            [TSExport]
            public class C1 : BaseC { public override int P { get; set; } }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, UnsupportedInheritanceId);
    }

    [Test]
    public async Task TSExportBaseClass_IsFlagged()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class BaseC { public int P { get; set; } }

            [TSExport]
            public class C1 : BaseC { }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, UnsupportedInheritanceId);
    }

    [Test]
    public async Task NonDisposableInterface_IsFlagged()
    {
        string source = """
            using TypeShim;

            public interface IThing { void Do(); }

            [TSExport]
            public class C1 : IThing { public void Do() { } }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, UnsupportedInheritanceId);
    }

    [Test]
    public async Task InterfaceExtendingDisposable_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            public interface IThing : IDisposable { void Do(); }

            [TSExport]
            public class C1 : IThing { public void Do() { } public void Dispose() { } }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, UnsupportedInheritanceId);
    }

    [Test]
    public async Task DisposableCombinedWithAnotherInterface_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            public interface IThing { void Do(); }

            [TSExport]
            public class C1 : IDisposable, IThing { public void Do() { } public void Dispose() { } }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, UnsupportedInheritanceId);
    }

    [Test]
    public async Task PlainClass_IsNotFlagged()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class C1 { public int P { get; set; } }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task DisposableOnly_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1 : IDisposable { public void Dispose() { } }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }
}
