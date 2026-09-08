using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using TypeShim.Analyzers;

namespace TypeShim.Analyzers.Tests;

internal static class AnalyzerTestHelper
{
    private static readonly ImmutableArray<MetadataReference> References = LoadReferences();

    // A minimal in-source [TSExport] attribute so the analyzer resolves it to TypeShim.TSExportAttribute.
    private const string TSExportAttributeSource = """
        namespace TypeShim { public sealed class TSExportAttribute : System.Attribute { } }
        """;

    private static ImmutableArray<MetadataReference> LoadReferences()
    {
        string tpa = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        return tpa.Split(Path.PathSeparator)
            .Where(path => !string.IsNullOrEmpty(path) && File.Exists(path))
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToImmutableArray();
    }

    internal static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "AnalyzerTests",
            [
                CSharpSyntaxTree.ParseText(source),
                CSharpSyntaxTree.ParseText(TSExportAttributeSource),
            ],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        CompilationWithAnalyzers withAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(new TypeShimAnalyzer()));

        return await withAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    // Asserts the analyzer emitted no diagnostics at all, so a test can't silently pass while an
    // unexpected diagnostic is also raised.
    internal static void AssertNoDiagnostics(ImmutableArray<Diagnostic> diagnostics)
    {
        Assert.That(diagnostics.Select(d => d.Id), Is.Empty, () => Describe(diagnostics));
    }

    // Asserts the analyzer emitted exactly one diagnostic and that it has the expected id.
    internal static void AssertSingleDiagnostic(ImmutableArray<Diagnostic> diagnostics, string expectedId)
    {
        Assert.That(diagnostics.Select(d => d.Id), Is.EqualTo(new[] { expectedId }), () => Describe(diagnostics));
    }

    // Asserts the analyzer emitted exactly the expected set of diagnostic ids (order-independent, counts matter).
    internal static void AssertDiagnostics(ImmutableArray<Diagnostic> diagnostics, params string[] expectedIds)
    {
        Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(expectedIds), () => Describe(diagnostics));
    }

    private static string Describe(ImmutableArray<Diagnostic> diagnostics)
    {
        if (diagnostics.IsEmpty)
            return "Expected diagnostics but none were emitted.";
        return "Actual diagnostics: " + string.Join(", ", diagnostics.Select(d => $"{d.Id}: {d.GetMessage()}"));
    }
}
