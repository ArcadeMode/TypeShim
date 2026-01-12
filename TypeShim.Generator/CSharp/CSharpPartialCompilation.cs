using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;

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
            // manually targetted assemblies
            typeof(object).Assembly,
            typeof(JSObject).Assembly,
            //typeof(TsExportAttribute).Assembly

            .. AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location)) // include loaded assemblies
        ];

        List<PortableExecutableReference> references = [.. baseAssemblies.Select(a => MetadataReference.CreateFromFile(a.Location))];
        return references;
    }


}
