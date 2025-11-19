// See https://aka.ms/new-console-template for more information
using DotnetWasmTypescript.InteropGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using TypeScriptExport;

Console.WriteLine("Hello, World!");
Console.WriteLine(string.Join("!!!", args));

string[] csFilePaths = args;
foreach (var file in csFilePaths)
{
    if (!File.Exists(file)) { }

    string code = File.ReadAllText(file);
    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);


    List<PortableExecutableReference> references = CsharpPartialCompilation.GetReferencesForSyntaxTree(syntaxTree);

    CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "TempAnalysis",
            syntaxTrees: new[] { syntaxTree },
            references: references);


    SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);


    SyntaxNode root = syntaxTree.GetRoot();
    IEnumerable<INamedTypeSymbol> typeSymbols = FindLabelledClassSymbols(semanticModel, root);
    foreach(INamedTypeSymbol classSymbol in typeSymbols) 
    {
        InteropClassBuilder interopClassBuilder = new();
        SourceText? source = interopClassBuilder.Build(classSymbol);
        if (source != null)
        {
            string outFileName = $"{classSymbol.Name}.Interop.cs";
            string outFileDir = Path.GetDirectoryName(file) ?? throw new InvalidOperationException($"Provided path {file} has no directory");
            File.WriteAllText(Path.Combine(outFileDir, outFileName), source.ToString());
        }
    }
}

static IEnumerable<INamedTypeSymbol> FindLabelledClassSymbols(SemanticModel semanticModel, SyntaxNode root)
{
    foreach (var cls in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
    {
        if(semanticModel.GetDeclaredSymbol(cls) is not INamedTypeSymbol symbol)
        {
            continue;
        }
        
        Console.WriteLine($"Found class: {symbol.Name}, Full name: {symbol.ToDisplayString()}");
        // Example: List attributes
        if (symbol.GetAttributes().Any(attributeData => attributeData.AttributeClass?.Name == nameof(TsExportAttribute)))
        {
            yield return symbol;
        }
    }
}