using Microsoft.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;

namespace TypeShim.Generator.CSharp;

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
            //typeof(TsExportAttribute).Assembly
        ];

        List<PortableExecutableReference> references = [.. baseAssemblies.Select(a => MetadataReference.CreateFromFile(a.Location))];
        return references;
    }

    
}
