using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;

namespace TypeShim.Generator.CSharp;

internal static class CSharpPartialCompilation
{
    internal static CSharpCompilation CreatePartialCompilation(IEnumerable<SyntaxTree> syntaxTrees, string runtimePackRefDir)
    {
        List<PortableExecutableReference> references = GetReferences(runtimePackRefDir);

        CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithNullableContextOptions(NullableContextOptions.Enable);

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "TempAnalysis",
            syntaxTrees: syntaxTrees,
            references: references,
            options: options);

        return compilation;
    }

    private static List<PortableExecutableReference> GetReferences(string runtimePackRefDir)
    {
        List<string> referenceAssemblyPaths = [
            $"{runtimePackRefDir}/System.Runtime.InteropServices.JavaScript.dll",
            $"{runtimePackRefDir}/System.Collections.dll",
            $"{runtimePackRefDir}/System.Runtime.dll",
        ]; ;

        if (referenceAssemblyPaths.Count == 0)
        {
            throw new InvalidOperationException("Failed to find reference assemblies, did you install dotnet?");
        }

        return [.. referenceAssemblyPaths.Select(s => MetadataReference.CreateFromFile(s))];
    }
}