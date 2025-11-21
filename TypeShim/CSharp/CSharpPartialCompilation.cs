using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using TypeScriptExport;
using Microsoft.CodeAnalysis.CSharp;

namespace TypeShim.CSharp;

internal static class CSharpPartialCompilation
{
   
    internal static CSharpCompilation CreatePartialCompilation(IEnumerable<SyntaxTree> syntaxTrees)
    {
        List<PortableExecutableReference> references = GetReferences();
        CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: "TempAnalysis",
                syntaxTrees: syntaxTrees,
                references: references);

        return compilation;
    }

    private static List<PortableExecutableReference> GetReferences()
    {
        // Always include mscorlib & system runtime (needed for core types)
        Assembly[] baseAssemblies =
        [
            typeof(object).Assembly,
            typeof(TsExportAttribute).Assembly
        ];

        List<PortableExecutableReference> references = [.. baseAssemblies.Select(a => MetadataReference.CreateFromFile(a.Location))];
        return references;
    }

    
}
