using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class OptionalConstructorParameterDiagnosticsTests
{
    private static readonly string RequiredInitializerId = TypeShimDiagnostics.NoOptionalCtorParamWithRequiredInitializerRule.Id;

    [Test]
    public async Task OptionalCtorParam_WithNonNullableProperty_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public C1(int count = 5) { }
                public string Name { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, RequiredInitializerId);
    }

    [Test]
    public async Task OptionalCtorParam_WithRequiredProperty_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public C1(int count = 5) { }
                public required string Name { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, RequiredInitializerId);
    }

    [Test]
    public async Task OptionalCtorParam_WithAllNullableProperties_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public C1(int count = 5) { }
                public string? Name { get; set; }
                public int? Age { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task OptionalCtorParam_WithGetOnlyProperty_IsNotFlagged()
    {
        // A get-only property produces no initializer object, so there is nothing to omit.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public C1(int count = 5) { }
                public string Name { get; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task RequiredCtorParam_WithRequiredProperty_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public C1(int count) { }
                public required string Name { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task OptionalMethodParam_WithRequiredProperty_IsNotFlagged()
    {
        // The rule is constructor-only; optional method parameters are unaffected.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public required string Name { get; set; }
                public void M(int count = 5) { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }
}
