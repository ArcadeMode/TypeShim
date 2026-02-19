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
            // TODO: throw if we cant find ref pack.
            Assembly[] baseAssemblies =
            [
                typeof(object).Assembly,
                typeof(JSObject).Assembly,
                .. AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location)),
            ];

            referenceAssemblyPaths = [.. baseAssemblies
                .Where(a => !string.IsNullOrEmpty(a.Location))
                .Select(a => a.Location)
                .Distinct(StringComparer.OrdinalIgnoreCase)];
        }

        return [.. referenceAssemblyPaths.Select(s => MetadataReference.CreateFromFile(s))];
    }

    private static List<string> TryGetNetCoreAppRefPackAssemblyPaths()
    {
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
            .FirstOrDefault();

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

        // Include all packed runtime DLLs.
        return [.. Directory.EnumerateFiles(refDir, "*.dll", SearchOption.TopDirectoryOnly)];
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
        // Prefer DOTNET_ROOT if present (common for SDK installs/CI).
        string? dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrWhiteSpace(dotnetRoot) && Directory.Exists(dotnetRoot))
        {
            return dotnetRoot;
        }

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