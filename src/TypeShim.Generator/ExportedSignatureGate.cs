using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Shared;

namespace TypeShim.Generator;

internal static class ExportedSignatureGate
{
    // Name/type-availability diagnostics that are false positives in the partial compilation
    // (missing refs / rewriter-stripped non-exported types or consts). These are reported with
    // better, name-specific messages by the parser and the analyzer. CS0103 covers a default
    // parameter value referencing a const declared in a stripped non-exported class.
    private static readonly HashSet<string> IgnoredDiagnosticIds =
        new(StringComparer.Ordinal) { "CS0246", "CS0234", "CS0122", "CS0103" };

    internal static void ThrowIfExportedSurfaceHasCompileErrors(
        CSharpCompilation compilation,
        IReadOnlyList<INamedTypeSymbol> exportedSymbols)
    {
        ThrowIfHasSyntaxErrors(exportedSymbols);
        ThrowIfHasErrorDiagnostic(compilation);
    }

    private static void ThrowIfHasErrorDiagnostic(CSharpCompilation compilation)
    {
        CSharpCompilation analysisCompilation = compilation.WithOptions(
            compilation.Options.WithNullableContextOptions(NullableContextOptions.Disable));

        foreach (Diagnostic diagnostic in analysisCompilation.GetDeclarationDiagnostics())
        {
            if (diagnostic.Severity != DiagnosticSeverity.Error || IgnoredDiagnosticIds.Contains(diagnostic.Id))
            {
                continue;
            }

            throw MakeException(diagnostic);
        }
    }

    private static void ThrowIfHasSyntaxErrors(IReadOnlyList<INamedTypeSymbol> exportedSymbols)
    {
        HashSet<SyntaxTree> declaringTrees = [];
        foreach (INamedTypeSymbol symbol in exportedSymbols)
        {
            foreach (SyntaxReference syntaxReference in symbol.DeclaringSyntaxReferences)
            {
                declaringTrees.Add(syntaxReference.SyntaxTree);
            }
        }

        foreach (SyntaxTree tree in declaringTrees)
        {
            foreach (Diagnostic diagnostic in tree.GetDiagnostics())
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    throw MakeException(diagnostic);
                }
            }
        }
    }

    private static InvalidCodeException MakeException(Diagnostic diagnostic)
    {
        FileLinePositionSpan position = diagnostic.Location.GetLineSpan();
        int line = position.StartLinePosition.Line + 1;
        int column = position.StartLinePosition.Character + 1;
        return new InvalidCodeException(
            $"TypeShim codegen aborted: invalid code in '{position.Path}' ({line},{column}): {diagnostic.Id} {diagnostic.GetMessage()}.");
    }
}
