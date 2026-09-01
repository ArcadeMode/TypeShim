using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
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
        ThrowIfHasSemanticErrors(compilation, exportedSymbols);
    }

    private static void ThrowIfHasSemanticErrors(CSharpCompilation compilation, IReadOnlyList<INamedTypeSymbol> exportedSymbols)
    {
        foreach (INamedTypeSymbol type in exportedSymbols)
        {
            foreach (ISymbol member in type.GetMembers())
            {
                if (member.DeclaredAccessibility != Accessibility.Public || member.IsImplicitlyDeclared)
                {
                    continue;
                }

                foreach (SyntaxReference syntaxReference in member.DeclaringSyntaxReferences)
                {
                    // Only check public signatures (return type, parameters, and constraints).
                    // The body of the member is not relevant to the interop surface and actually
                    // is very likely to contain many errors due to non TSExport symbol stripping (perf)
                    if (!TryGetSignatureSpan(syntaxReference.GetSyntax(), out TextSpan span))
                    {
                        continue;
                    }

                    SemanticModel model = compilation.GetSemanticModel(syntaxReference.SyntaxTree);
                    foreach (Diagnostic diagnostic in model.GetDiagnostics(span))
                    {
                        if (diagnostic.Severity == DiagnosticSeverity.Error && !IgnoredDiagnosticIds.Contains(diagnostic.Id))
                        {
                            throw MakeException(diagnostic);
                        }
                    }
                }
            }
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

    /// <summary>
    /// Signature span anchored at the return/element type (excludes attributes and modifiers) and
    /// extending to the end of the parameter list, or the constraint clauses when present.
    /// Constructors have no return type, so they anchor at the identifier. Returns <c>false</c> for
    /// members that are not codegen inputs (fields, events, nested types, ...).
    /// </summary>
    private static bool TryGetSignatureSpan(SyntaxNode node, out TextSpan span)
    {
        switch (node)
        {
            case MethodDeclarationSyntax method:
            {
                int end = method.ConstraintClauses.Count > 0
                    ? method.ConstraintClauses[^1].Span.End
                    : method.ParameterList.Span.End;
                span = TextSpan.FromBounds(method.ReturnType.SpanStart, end);
                return true;
            }
            case ConstructorDeclarationSyntax constructor:
                span = TextSpan.FromBounds(constructor.Identifier.SpanStart, constructor.ParameterList.Span.End);
                return true;
            case IndexerDeclarationSyntax indexer:
                span = TextSpan.FromBounds(indexer.Type.SpanStart, indexer.ParameterList.Span.End);
                return true;
            case PropertyDeclarationSyntax property:
                span = TextSpan.FromBounds(property.Type.SpanStart, property.Identifier.Span.End);
                return true;
            default:
                span = default;
                return false;
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
