using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptEnumRendererTests
{
    private static List<NamedTypeInfo> BuildAll(string source)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
        SymbolExtractor extractor = new([CSharpFileInfo.Create(tree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> symbols = [.. extractor.ExtractAllExportedSymbols()];
        InteropTypeInfoCache cache = new();
        return [.. symbols.Select(s => new NamedTypeInfoBuilder(s, cache).Build()).OfType<NamedTypeInfo>()];
    }

    private static string RenderEnum(string source, string enumName = "Color")
    {
        List<NamedTypeInfo> all = BuildAll(source);
        EnumInfo enumInfo = all.OfType<EnumInfo>().First(e => e.Name == enumName);
        RenderContext ctx = new(enumInfo, all, RenderOptions.TypeScript);
        new TypeScriptEnumRenderer(ctx).Render();
        return ctx.ToString();
    }

    [Test]
    public void EnumRenderer_RendersMembersWithExactValues()
    {
        string ts = RenderEnum("""
            namespace N1;
            [TSExport]
            public enum Color { Red, Green = 5, Blue }
        """);

        AssertEx.EqualOrDiff(ts, """
export enum Color {
  Red = 0,
  Green = 5,
  Blue = 6,
}

""");
    }

    [Test]
    public void EnumRenderer_RendersJSDocForEnumAndMembers()
    {
        string ts = RenderEnum("""
            namespace N1;
            /// <summary>A color.</summary>
            [TSExport]
            public enum Color
            {
                /// <summary>The red one.</summary>
                Red,
                Green
            }
        """);

        AssertEx.EqualOrDiff(ts, """
/**
 * A color.
 */
export enum Color {
  /**
   * The red one.
   */
  Red = 0,
  Green = 1,
}

""");
    }

    [Test]
    public void EnumRenderer_RendersNegativeAndFlagCombinationValues()
    {
        string ts = RenderEnum("""
            using System;
            namespace N1;
            [Flags]
            [TSExport]
            public enum Color { None = 0, A = 1, B = 2, AB = A | B, Neg = -1 }
        """);

        AssertEx.EqualOrDiff(ts, """
export enum Color {
  None = 0,
  A = 1,
  B = 2,
  AB = 3,
  Neg = -1,
}

""");
    }

    [Test]
    public void TypeScriptRenderer_EmitsEnumDeclarationWithHeaderComment()
    {
        List<NamedTypeInfo> all = BuildAll("""
            namespace N1;
            [TSExport]
            public enum Color { Red, Green, Blue }
        """);
        ModuleInfo moduleInfo = new()
        {
            ExportedClasses = [.. all.OfType<ClassInfo>()],
            HierarchyInfo = ModuleHierarchyInfo.FromClasses([.. all.OfType<ClassInfo>()])
        };

        List<RenderContext> contexts = new TypeScriptRenderer(all, moduleInfo).Render();
        // Contexts: [0] config preamble, [1] assembly exports, [2] the enum
        string enumContext = contexts[2].ToString();

        AssertEx.EqualOrDiff(enumContext, """
// TypeShim generated TypeScript definitions for enum: N1.Color
export enum Color {
  Red = 0,
  Green = 1,
  Blue = 2,
}

""");
    }
}
