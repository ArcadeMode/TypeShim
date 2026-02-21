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
    internal static CSharpCompilation CreatePartialCompilation(IEnumerable<SyntaxTree> syntaxTrees)
    {
        List<PortableExecutableReference> references = GetReferences();

        CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithNullableContextOptions(NullableContextOptions.Enable);

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "TempAnalysis",
            syntaxTrees: syntaxTrees,
            references: references,
            options: options);

        return compilation;
    }

    private static List<PortableExecutableReference> GetReferences()
    {
        List<string> referenceAssemblyPaths = TryGetNetCoreAppRefPackAssemblyPaths();

        if (referenceAssemblyPaths.Count == 0)
        {
            throw new InvalidOperationException("Failed to find reference assemblies, did you install dotnet?");
        }

        return [.. referenceAssemblyPaths.Select(s => MetadataReference.CreateFromFile(s))];
    }

    private static List<string> TryGetNetCoreAppRefPackAssemblyPaths()
    {
        // GetDotnetRoot is EXPENSIVE 
        string? dotnetRoot = GetDotnetRoot();
        if (string.IsNullOrWhiteSpace(dotnetRoot))
        {
            return [];
        }

        string packsRoot = Path.Combine(dotnetRoot, "packs", "Microsoft.NETCore.App.Ref");
        if (!Directory.Exists(packsRoot))
        {
            return [];
        }

        DirectoryInfo packsDir = new(packsRoot);
        DirectoryInfo? bestVersionDir = packsDir
            .EnumerateDirectories()
            .Select(d => new { Dir = d, Version = TryParseVersionFromDirectoryName(d.Name) })
            .Where(x => x.Version is not null)
            .OrderByDescending(x => x.Version)
            .Select(x => x.Dir)
            .FirstOrDefault(); // FILE ATTRIBUTE LOADING is EXPENSIVE

        // TODO: come up with smarter way to find the `refDir`, equally expensive as writing 10 generated csharp files!

        if (bestVersionDir is null)
        {
            return [];
        }

        string[] tfmCandidates = // TODO: only use target candidate (make input for generator cli?)
        [
            "net10.0",
            "net9.0",
            "net8.0",
        ];

        string? refDir = tfmCandidates
            .Select(tfm => Path.Combine(bestVersionDir.FullName, "ref", tfm))
            .FirstOrDefault(Directory.Exists);

        if (refDir is null)
        {
            return [];
        }

        // Include _minimal_ dlls, to keep the codegen snappy
        return [
            $"{refDir}/System.Runtime.InteropServices.JavaScript.dll",
            $"{refDir}/System.Collections.dll",
            $"{refDir}/System.Runtime.dll",
        ];
    }

    private static Version? TryParseVersionFromDirectoryName(string name)
    {
        // Packs can look like "10.0.0-preview.1.25100.1", remove suffix.
        string[] parts = name.Split('-', 2, StringSplitOptions.TrimEntries);
        if (Version.TryParse(parts[0], out Version? version))
        {
            return version;
        }

        return null;
    }

    private static string? GetDotnetRoot()
    {
        // Windows fallbacks
        string? programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (!string.IsNullOrWhiteSpace(programFiles))
        {
            string candidate = Path.Combine(programFiles, "dotnet");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }
        string? programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (!string.IsNullOrWhiteSpace(programFilesX86))
        {
            string candidate = Path.Combine(programFilesX86, "dotnet");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}