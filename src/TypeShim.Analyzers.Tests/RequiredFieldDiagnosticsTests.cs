using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class RequiredFieldDiagnosticsTests
{
    private static readonly string RequiredFieldId = TypeShimDiagnostics.NoRequiredFieldsRule.Id;

    [Test]
    public async Task PublicRequiredField_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public required int F;
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, RequiredFieldId);
    }

    [Test]
    public async Task PublicNonRequiredField_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public int F;
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task RequiredProperty_IsNotFlaggedAsRequiredField()
    {
        // The rule is field-specific; required properties are allowed.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public required int P { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task NonPublicRequiredField_IsNotFlagged()
    {
        // Only public required fields are banned.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                internal required int F;
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task ConstField_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public const int F = 1;
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }


    [Test]
    public async Task RequiredField_InsideNestedType_IsFlagged()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class Outer
            {
                public class Inner
                {
                    public required int Field;
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, RequiredFieldId);
    }
}
