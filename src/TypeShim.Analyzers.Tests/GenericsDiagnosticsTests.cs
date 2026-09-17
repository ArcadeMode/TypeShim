using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class GenericsDiagnosticsTests
{
    private static readonly string GenericClassId = TypeShimDiagnostics.NoGenericsTSExportRule.Id;
    private static readonly string GenericMethodId = TypeShimDiagnostics.NoGenericsPublicMethodRule.Id;

    [Test]
    public async Task GenericTSExportClass_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1<T>
            {
                public int P { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, GenericClassId);
    }

    [Test]
    public async Task MultiParameterGenericTSExportClass_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1<T, U>
            {
                public int P { get; set; }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, GenericClassId);
    }

    [Test]
    public async Task NonGenericTSExportClass_IsNotFlagged()
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
    public async Task GenericPublicMethod_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public int M<T>(int x) => x;
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, GenericMethodId);
    }

    [Test]
    public async Task MultiParameterGenericPublicMethod_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public void M<T, U>() { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, GenericMethodId);
    }

    [Test]
    public async Task NonGenericPublicMethod_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public int M(int x) => x;
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task GenericNonPublicMethod_IsNotFlagged()
    {
        // Only public methods are checked for type parameters.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                private T M<T>(T x) => x;
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }


    [Test]
    public async Task GenericMethod_InsideNestedType_IsFlagged()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class Outer
            {
                public class Inner
                {
                    public void M<T>() { }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, GenericMethodId);
    }
}
