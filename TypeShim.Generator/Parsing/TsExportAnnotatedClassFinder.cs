using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace TypeShim.Generator.Parsing;

internal sealed class TsExportAnnotatedClassFinder
{
    internal static IEnumerable<INamedTypeSymbol> FindLabelledClassSymbols(SemanticModel semanticModel, SyntaxNode root)
    {
        foreach (var cls in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            if (semanticModel.GetDeclaredSymbol(cls) is not INamedTypeSymbol symbol)
            {
                continue;
            }

            if (symbol.GetAttributes().Any(attributeData => attributeData.AttributeClass?.Name is "TsExportAttribute" or "TsExport"))
            {
                //TODO: add verbosity argument and use with ILogger
                //Console.WriteLine($"TsExport: {symbol.ToDisplayString()}");
                yield return symbol;
            }
        }
    }
}
