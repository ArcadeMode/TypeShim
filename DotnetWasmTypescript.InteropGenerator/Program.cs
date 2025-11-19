using DotnetWasmTypescript.InteropGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TypeScriptExport;

string[] csFilePaths = args;
foreach (string csFilePath in csFilePaths)
{
    if (!File.Exists(csFilePath)) {
        throw new InvalidOperationException($"Invalid .cs file path provided 'file'");
    }

    string code = File.ReadAllText(csFilePath);
    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
    SemanticModel semanticModel = CSharpPartialCompilation.CreatePartialCompilation(syntaxTree);
    SyntaxNode root = syntaxTree.GetRoot();
    IEnumerable<INamedTypeSymbol> typeSymbols = FindLabelledClassSymbols(semanticModel, root);
    foreach(INamedTypeSymbol classSymbol in typeSymbols) 
    {
        InteropClassBuilder interopClassBuilder = new();
        SourceText? source = interopClassBuilder.Build(classSymbol);
        if (source != null)
        {
            string outFileName = $"{classSymbol.Name}.Interop.cs";
            string outFileDir = Path.GetDirectoryName(csFilePath) ?? throw new InvalidOperationException($"Provided path {csFilePath} has no directory");
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
        
        // Example: List attributes
        if (symbol.GetAttributes().Any(attributeData => attributeData.AttributeClass?.Name == nameof(TsExportAttribute)))
        {
            Console.WriteLine($"TsExport: {symbol.ToDisplayString()}");
            yield return symbol;
        }
    }
}