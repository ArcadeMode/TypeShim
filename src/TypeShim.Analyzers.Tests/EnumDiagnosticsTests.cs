using System.Linq;
using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class EnumDiagnosticsTests
{
    private static readonly string NonExportedTypeId = TypeShimDiagnostics.NonExportedTypeInInteropApiRule.Id;
    private static readonly string UnsupportedTypeId = TypeShimDiagnostics.UnsupportedTypeRule.Id;
    private static readonly string ConstScopeId = TypeShimDiagnostics.UnresolvableDefaultConstRule.Id;

    [Test]
    public async Task UnannotatedEnumParameter_IsFlaggedAsNonExported()
    {
        // A non-[TSExport] enum on the interop boundary is stripped from codegen and fails there,
        // so the analyzer must flag it up front, exactly like a non-exported class.
        string source = """
            using System;
            using TypeShim;

            public enum Priority { Low, Medium, High }

            [TSExport]
            public class C1
            {
                public void M(Priority p) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == NonExportedTypeId), Is.True);
    }

    [Test]
    public async Task UnannotatedEnumReturnType_IsFlaggedAsNonExported()
    {
        string source = """
            using System;
            using TypeShim;

            public enum Priority { Low, Medium, High }

            [TSExport]
            public class C1
            {
                public Priority M() => Priority.Low;
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == NonExportedTypeId), Is.True);
    }

    [Test]
    public async Task UnannotatedEnumProperty_IsFlaggedAsNonExported()
    {
        string source = """
            using System;
            using TypeShim;

            public enum Priority { Low, Medium, High }

            [TSExport]
            public class C1
            {
                public Priority P { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == NonExportedTypeId), Is.True);
    }

    [Test]
    public async Task UnannotatedEnumArrayParameter_IsFlaggedAsNonExported()
    {
        string source = """
            using System;
            using TypeShim;

            public enum Priority { Low, Medium, High }

            [TSExport]
            public class C1
            {
                public void M(Priority[] values) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == NonExportedTypeId), Is.True);
    }

    [Test]
    public async Task UnannotatedNullableEnumParameter_IsFlaggedAsNonExported()
    {
        string source = """
            using System;
            using TypeShim;

            public enum Priority { Low, Medium, High }

            [TSExport]
            public class C1
            {
                public void M(Priority? p = null) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == NonExportedTypeId), Is.True);
    }

    [Test]
    public async Task TSExportEnumParameter_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public enum Priority { Low, Medium, High }

            [TSExport]
            public class C1
            {
                public void M(Priority p) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == NonExportedTypeId || d.Id == UnsupportedTypeId), Is.False);
    }

    [Test]
    public async Task TSExportEnumProperty_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public enum Priority { Low, Medium, High }

            [TSExport]
            public class C1
            {
                public Priority P { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == NonExportedTypeId || d.Id == UnsupportedTypeId), Is.False);
    }

    [Test]
    public async Task OptionalEnumParameterDefault_FromTSExportEnum_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public enum Priority { Low, Medium, High }

            [TSExport]
            public class C1
            {
                public void M(Priority p = Priority.Low) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == ConstScopeId), Is.False);
    }

    [Test]
    public async Task UnsupportedUnderlyingEnumParameter_IsFlaggedAsUnsupported()
    {
        // Unsigned underlying types cannot cross the .NET-JS boundary, so the enum itself is unsupported
        // regardless of export status.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public enum Priority : uint { Low, Medium, High }

            [TSExport]
            public class C1
            {
                public void M(Priority p) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == UnsupportedTypeId), Is.True);
    }
}
