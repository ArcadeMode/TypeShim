using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.CSharp;

internal class CSharpInteropClassRendererTests_JSExport
{
    [Test]
    public void CSharpInteropClass_JSExportOnlyClass_GeneratesNothing()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static int M1() => 1;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        InteropTypeInfoCache typeInfoCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(classSymbol, typeInfoCache).Build();
        RenderContext renderContext = new(classInfo, [classInfo], RenderOptions.CSharp);
        string interopClass = new CSharpInteropClassRenderer(classInfo, renderContext, new JSObjectMethodResolver([])).Render();

        // No C# interop wrapper should be generated for a JSExport-only class -
        // the methods are already WASM-exported by Roslyn's source generator.
        Assert.That(interopClass.Trim(), Is.Empty);
    }
}
