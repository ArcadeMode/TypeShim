using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TypeScriptExport;

namespace DotnetWasmTypescript.InteropGenerator;

internal class CsharpPartialCompilation
{
    /// <summary>
    /// Constructs a list of MetadataReferences required to compile the given syntax tree.
    /// Automatically includes common core assemblies and attempts to locate additional assemblies for each using directive.
    /// </summary>
    public static List<PortableExecutableReference> GetReferencesForSyntaxTree(SyntaxTree syntaxTree)
    {
        // Find all using directives
        SyntaxNode root = syntaxTree.GetRoot();
        List<string> usingDirectives = [.. root
            .DescendantNodes()
            .OfType<UsingDirectiveSyntax>()
            .Select(u => u.Name?.ToString() ?? string.Empty)
            .Distinct()];

        // Always include mscorlib & system runtime (needed for core types)
        Assembly[] baseAssemblies = new[]
        {
            typeof(object).Assembly,
            typeof(Console).Assembly,
            typeof(Enumerable).Assembly,
            typeof(TsExportAttribute).Assembly
        };

        // Try to map namespace to an assembly in the current AppDomain
        Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<Assembly?> matchedAssemblies = usingDirectives
            .Select(ns =>
                loadedAssemblies.FirstOrDefault(a =>
                    a.GetTypes().Any(t => t.Namespace == ns))
            )
            .Where(a => a != null)
            .ToList();

        // Combine and deduplicate
        IEnumerable<Assembly?> allAssemblies = baseAssemblies.Concat(matchedAssemblies).Distinct();

        List<PortableExecutableReference> references = [.. allAssemblies.Select(a => MetadataReference.CreateFromFile(a.Location))];

        return references;
    }
}
