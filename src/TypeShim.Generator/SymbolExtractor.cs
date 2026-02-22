using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator;

internal class SymbolExtractor(CSharpFileInfo[] fileInfos, string runtimePackRefDir)
{
    internal List<INamedTypeSymbol> ExtractAllExportedSymbols()
    {
        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation(GetExportOnlyTrees(), runtimePackRefDir);
        List<INamedTypeSymbol> exportedSymbols = new(fileInfos.Length * 2); // should be enough space to avoid having to extend the list
        FindExports(compilation.Assembly.GlobalNamespace, exportedSymbols);
        return exportedSymbols;
    }

    private IEnumerable<SyntaxTree> GetExportOnlyTrees()
    {
        foreach (CSharpFileInfo fileInfo in fileInfos)
        {
            TSExportOnlySyntaxRewriter rewriter = new();
            CompilationUnitSyntax root = fileInfo.SyntaxTree.GetCompilationUnitRoot();
            if (rewriter.Visit(root) is not CompilationUnitSyntax syntax)
            {
                continue;
            }
            yield return fileInfo.SyntaxTree.WithRootAndOptions(syntax, fileInfo.SyntaxTree.Options);
        }
    }

    private static void FindExports(INamespaceSymbol ns, List<INamedTypeSymbol> exportedSymbols)
    {
        foreach (INamedTypeSymbol typeMember in ns.GetTypeMembers())
        {
            exportedSymbols.Add(typeMember);
        }

        foreach (INamespaceSymbol namespaceSymbol in ns.GetNamespaceMembers())
        {
            FindExports(namespaceSymbol, exportedSymbols);
        }
    }
}
