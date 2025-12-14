using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator;

internal class SymbolExtractor(IEnumerable<CSharpFileInfo> fileInfos)
{
    internal IEnumerable<INamedTypeSymbol> ExtractAllExportedSymbols()
    {
        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation(fileInfos.Select(csFile => csFile.SyntaxTree));

        List<INamedTypeSymbol> classInfos = [.. fileInfos.SelectMany(fileInfo => FindLabelledClassSymbols(compilation.GetSemanticModel(fileInfo.SyntaxTree), fileInfo.SyntaxTree.GetRoot()))];
        return classInfos;

    }

    private static IEnumerable<INamedTypeSymbol> FindLabelledClassSymbols(SemanticModel semanticModel, SyntaxNode root)
    {
        foreach (var cls in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            if (semanticModel.GetDeclaredSymbol(cls) is not INamedTypeSymbol symbol)
            {
                continue;
            }

            if (symbol.GetAttributes().Any(attributeData => attributeData.AttributeClass?.Name is "TSExportAttribute" or "TSExport" or "TSModuleAttribute" or "TSModule"))
            {
                //TODO: add verbosity argument and use with ILogger
                //Console.WriteLine($"TsExport: {symbol.ToDisplayString()}");
                yield return symbol;
            }
        }
    }
}
