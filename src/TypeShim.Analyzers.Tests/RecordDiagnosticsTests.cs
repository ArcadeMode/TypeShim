using System.Linq;
using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class RecordDiagnosticsTests
{
    private static readonly string RecordNotSupportedId = TypeShimDiagnostics.RecordNotSupportedRule.Id;
    private static readonly string NoOverloadsId = TypeShimDiagnostics.NoOverloadsRule.Id;

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
        Assert.That(diagnostics.Any(d => d.Id == RecordNotSupportedId), Is.True);
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
        Assert.That(diagnostics.Any(d => d.Id == RecordNotSupportedId), Is.True);
    }

    [Test]
    public async Task TSExportRecord_IsNotFlaggedForSynthesizedEqualsOverload()
    {
        // Rejecting the record up front must suppress the misleading overload diagnostic that the
        // synthesized Equals method would otherwise trigger.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public record C1(string Name);
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == NoOverloadsId), Is.False);
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
        Assert.That(diagnostics.Any(d => d.Id == RecordNotSupportedId), Is.False);
    }
}
