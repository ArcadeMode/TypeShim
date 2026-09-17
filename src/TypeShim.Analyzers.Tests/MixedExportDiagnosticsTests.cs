using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class MixedExportDiagnosticsTests
{
    private static readonly string MixedExportId = TypeShimDiagnostics.MixedExportRule.Id;

    // The analyzer resolves [JSExport] by its fully-qualified name, so provide a matching in-source stub.
    private const string JSExportAttributeSource = """
        namespace System.Runtime.InteropServices.JavaScript
        {
            public sealed class JSExportAttribute : System.Attribute { }
        }
        """;

    [Test]
    public async Task JSExportMethodInTSExportClass_IsFlagged()
    {
        string source = """
            using System;
            using System.Runtime.InteropServices.JavaScript;
            using TypeShim;

            [TSExport]
            public partial class C1
            {
                [JSExport]
                public static void M() { }
            }
            """ + "\n" + JSExportAttributeSource;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, MixedExportId);
    }

    [Test]
    public async Task MultipleJSExportMethodsInTSExportClass_AreEachFlagged()
    {
        string source = """
            using System;
            using System.Runtime.InteropServices.JavaScript;
            using TypeShim;

            [TSExport]
            public partial class C1
            {
                [JSExport]
                public static void M() { }

                [JSExport]
                public static void N() { }
            }
            """ + "\n" + JSExportAttributeSource;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertDiagnostics(diagnostics, MixedExportId, MixedExportId);
    }

    [Test]
    public async Task JSExportMethodWithoutTSExportClass_IsNotFlagged()
    {
        string source = """
            using System;
            using System.Runtime.InteropServices.JavaScript;

            public partial class C1
            {
                [JSExport]
                public static void M() { }
            }
            """ + "\n" + JSExportAttributeSource;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    [Test]
    public async Task TSExportClassWithoutJSExport_IsNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class C1
            {
                public static void M() { }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }


    [Test]
    public async Task JSExportMethodInNestedTypeUnderTSExportContainer_IsFlagged()
    {
        string source = """
            using System;
            using System.Runtime.InteropServices.JavaScript;
            using TypeShim;

            [TSExport]
            public partial class Outer
            {
                public partial class Inner
                {
                    [JSExport]
                    public static void M() { }
                }
            }
            """ + "\n" + JSExportAttributeSource;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, MixedExportId);
    }
}
