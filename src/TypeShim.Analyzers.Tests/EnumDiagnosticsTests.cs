using System.Linq;
using System.Threading.Tasks;

namespace TypeShim.Analyzers.Tests;

internal class EnumDiagnosticsTests
{
    private static readonly string NonExportedTypeId = TypeShimDiagnostics.NonExportedTypeInInteropApiRule.Id;
    private static readonly string UnsupportedTypeId = TypeShimDiagnostics.UnsupportedTypeRule.Id;
    private static readonly string ConstScopeId = TypeShimDiagnostics.UnresolvableDefaultConstRule.Id;
    private static readonly string MemberOutOfRangeId = TypeShimDiagnostics.EnumMemberOutOfSafeRangeRule.Id;

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

    [Test]
    public async Task ULongUnderlyingEnum_IsFlaggedAsUnsupported_NamingUnderlyingType()
    {
        // ulong is unsigned and cannot cross the .NET-JS boundary; the message must name the offending
        // underlying type so the fix (switch to a signed type) is obvious.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public enum Priority : ulong { Low, Medium, High }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == UnsupportedTypeId && d.GetMessage().Contains("ulong")), Is.True);
    }

    [TestCase("ulong")]
    [TestCase("uint")]
    [TestCase("ushort")]
    [TestCase("sbyte")]
    public async Task UnsupportedUnderlyingEnum_IsFlagged_NamingUnderlyingType(string underlying)
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public enum Priority : {{underlying}} { Low, Medium, High }
            """.Replace("{{underlying}}", underlying);

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == UnsupportedTypeId && d.GetMessage().Contains(underlying)), Is.True);
    }

    [TestCase("byte")]
    [TestCase("short")]
    [TestCase("int")]
    [TestCase("long")]
    public async Task SupportedUnderlyingEnum_IsNotFlaggedAsUnsupported(string underlying)
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public enum Priority : {{underlying}} { Low, Medium, High }

            [TSExport]
            public class C1
            {
                public void M(Priority p) { }
            }
            """.Replace("{{underlying}}", underlying);

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == UnsupportedTypeId), Is.False);
    }

    [Test]
    public async Task UnsupportedUnderlyingEnum_DoesNotAlsoReportMemberOutOfRange()
    {
        // The underlying-type error short-circuits member inspection: the members may not even fit in Int64.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public enum Priority : ulong { Low = 0, High = 18446744073709551615 }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == MemberOutOfRangeId), Is.False);
    }

    [Test]
    public async Task LongEnumMemberAboveSafeRange_IsFlaggedAsOutOfRange()
    {
        // 9007199254740992 == 2^53, the first value above the JS-safe range.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public enum Big : long { Zero = 0, Max = 9007199254740992 }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == MemberOutOfRangeId && d.GetMessage().Contains("Max")), Is.True);
    }

    [Test]
    public async Task LongEnumMemberBelowSafeRange_IsFlaggedAsOutOfRange()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public enum Big : long { Zero = 0, Min = -9007199254740992 }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == MemberOutOfRangeId && d.GetMessage().Contains("Min")), Is.True);
    }

    [Test]
    public async Task LongEnumMemberAtSafeBoundary_IsNotFlagged()
    {
        // 9007199254740991 == 2^53 - 1, the largest exactly-representable JS integer; the boundary is inclusive.
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public enum Big : long { Small = 0, Max = 9007199254740991, Min = -9007199254740991 }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == MemberOutOfRangeId), Is.False);
    }

    [Test]
    public async Task MultipleOutOfRangeEnumMembers_AreEachFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public enum Big : long { Zero = 0, TooBig = 9007199254740992, TooSmall = -9007199254740992 }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Count(d => d.Id == MemberOutOfRangeId), Is.EqualTo(2));
    }

    [Test]
    public async Task InRangeEnumMembers_AreNotFlagged()
    {
        string source = """
            using System;
            using TypeShim;

            [TSExport]
            public enum Priority { Low, Medium, High }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == MemberOutOfRangeId), Is.False);
    }

    [Test]
    public async Task NonExportedEnum_WithOutOfRangeMember_IsNotFlaggedForRange()
    {
        // Member-range validation only applies to [TSExport] enums that are actually emitted.
        string source = """
            using System;
            using TypeShim;

            public enum Big : long { Zero = 0, Max = 9007199254740992 }
            """;

        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(source);
        Assert.That(diagnostics.Any(d => d.Id == MemberOutOfRangeId), Is.False);
    }
}
