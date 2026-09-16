using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class NestedTypeDiagnosticsTests
{
    private static readonly string UnsupportedTypeId = TypeShimDiagnostics.UnsupportedTypeRule.Id;
    private static readonly string NonExportedTypeId = TypeShimDiagnostics.NonExportedTypeInInteropApiRule.Id;
    private static readonly string NoOverloadsId = TypeShimDiagnostics.NoOverloadsRule.Id;
    private static readonly string NoRequiredFieldsId = TypeShimDiagnostics.NoRequiredFieldsRule.Id;
    private static readonly string NoGenericsPublicMethodId = TypeShimDiagnostics.NoGenericsPublicMethodRule.Id;
    private static readonly string NoOptionalMemoryViewId = TypeShimDiagnostics.NoOptionalMemoryViewRule.Id;
    private static readonly string EnumOutOfRangeId = TypeShimDiagnostics.EnumMemberOutOfSafeRangeRule.Id;
    private static readonly string UnsupportedInheritanceId = TypeShimDiagnostics.UnsupportedInheritanceRule.Id;
    private static readonly string MixedExportId = TypeShimDiagnostics.MixedExportRule.Id;
    private static readonly string NestedWithoutContainerId = TypeShimDiagnostics.NestedTSExportWithoutExportedContainerRule.Id;
    private static readonly string RedundantNestedId = TypeShimDiagnostics.RedundantNestedTSExportRule.Id;

    // --- Nesting is respected: valid nested types produce no diagnostics ---

    [Test]
    public async Task ValidNestedType_UnderExportedContainer_ProducesNoDiagnostics()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class Outer
            {
                public int Id { get; set; }

                public class Inner
                {
                    public int Value { get; set; }
                    public int Compute() => Value;
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    // --- Member-level diagnostics now fire inside nested types ---

    [Test]
    public async Task UnsupportedType_InsideNestedType_IsFlagged()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class Outer
            {
                public class Inner
                {
                    public decimal M() => 0m;
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, UnsupportedTypeId);
    }

    [Test]
    public async Task NonExportedType_InsideNestedType_IsFlagged()
    {
        string source = """
            using TypeShim;

            public class Other { }

            [TSExport]
            public class Outer
            {
                public class Inner
                {
                    public Other M() => new Other();
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, NonExportedTypeId);
    }

    [Test]
    public async Task Overloads_InsideNestedType_AreFlagged()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class Outer
            {
                public class Inner
                {
                    public void M() { }
                    public void M(int x) { }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, NoOverloadsId);
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
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, NoRequiredFieldsId);
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
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, NoGenericsPublicMethodId);
    }

    [Test]
    public async Task OptionalMemoryViewParameter_InsideNestedType_IsFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public class Outer
            {
                public class Inner
                {
                    public void M(Span<byte> data = default) { }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, NoOptionalMemoryViewId);
    }

    [Test]
    public async Task EnumMemberOutOfRange_InNestedEnum_IsFlagged()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class Outer
            {
                public enum Kind : long
                {
                    Big = 9007199254740992
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, EnumOutOfRangeId);
    }

    [Test]
    public async Task Inheritance_OnNestedType_IsFlagged()
    {
        string source = """
            using TypeShim;

            public class BaseC { public int P { get; set; } }

            [TSExport]
            public class Outer
            {
                public class Inner : BaseC
                {
                    public int Value { get; set; }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, UnsupportedInheritanceId);
    }

    [Test]
    public async Task MixedExport_OnNestedTypeMethod_IsFlagged()
    {
        const string jsExportAttributeSource = """
            namespace System.Runtime.InteropServices.JavaScript
            {
                public sealed class JSExportAttribute : System.Attribute { }
            }
            """;
        string source = """
            using System.Runtime.InteropServices.JavaScript;
            using TypeShim;

            [TSExport]
            public class Outer
            {
                public class Inner
                {
                    [JSExport]
                    public static void M() { }
                }
            }
            """ + "\n" + jsExportAttributeSource;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, MixedExportId);
    }

    [Test]
    public async Task MultiLevelNesting_MemberDiagnostics_StillFire()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class Outer
            {
                public class Middle
                {
                    public class Inner
                    {
                        public decimal M() => 0m;
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertSingleDiagnostic(diagnostics, UnsupportedTypeId);
    }

    // --- Non-public nested types under an exported container are not analyzed ---

    [Test]
    public async Task PrivateNestedType_UnderExportedContainer_IsNotAnalyzed()
    {
        string source = """
            using TypeShim;

            [TSExport]
            public class Outer
            {
                public int Id { get; set; }

                private class Hidden
                {
                    public decimal M() => 0m;
                }
            }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        AnalyzerTestHelper.AssertNoDiagnostics(diagnostics);
    }

    // --- TSHIM020 / TSHIM021 nested [TSExport] annotation diagnostics ---

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
    public async Task NestedTSExport_UnderNonExportedTopLevel_TreatsAncestorChainAsNotExported()
    {
        // The direct container is [TSExport] but the top-level container is not; the inner type inherits
        // exportedness from Middle, so [TSExport] on it is redundant rather than orphaned.
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
        // Middle: no [TSExport] ancestor -> warning. Inner: Middle is a [TSExport] ancestor -> redundant.
        AnalyzerTestHelper.AssertDiagnostics(diagnostics, NestedWithoutContainerId, RedundantNestedId);
    }
}
