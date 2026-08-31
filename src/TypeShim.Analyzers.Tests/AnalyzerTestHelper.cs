using System.Collections.Immutable;
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
}
