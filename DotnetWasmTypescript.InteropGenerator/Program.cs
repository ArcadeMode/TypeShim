using DotnetWasmTypescript.InteropGenerator;
using DotnetWasmTypescript.InteropGenerator.Typescript;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using System.Reflection;
using System.Text;
using TypeScriptExport;

string[] csFilePaths = args;

List<CSharpFileInfo> fileInfos = new();
foreach (string csFilePath in csFilePaths)
{
    if (!File.Exists(csFilePath)) {
        throw new InvalidOperationException($"Invalid .cs file path provided '{csFilePath}'");
    }

    string code = File.ReadAllText(csFilePath);
    fileInfos.Add(new CSharpFileInfo 
    { 
        SyntaxTree = CSharpSyntaxTree.ParseText(code), 
        Path = csFilePath 
    });
}

CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation(fileInfos.Select(i => i.SyntaxTree));

List<(CSharpFileInfo FileInfo, ClassInfo ClassInfo)> classInfoByFile = [];
foreach (CSharpFileInfo fileInfo in fileInfos)
{
    SemanticModel semanticModel = compilation.GetSemanticModel(fileInfo.SyntaxTree);
    SyntaxNode root = fileInfo.SyntaxTree.GetRoot();
    IEnumerable<INamedTypeSymbol> typeSymbols = FindLabelledClassSymbols(semanticModel, root);
    foreach (INamedTypeSymbol classSymbol in typeSymbols)
    {
        ClassInfoBuilder classInfoBuilder = new(classSymbol);
        ClassInfo classInfo = classInfoBuilder.Build();

        classInfoByFile.Add((fileInfo, classInfo));
    }
}


foreach ((CSharpFileInfo fileInfo, ClassInfo classInfo) in classInfoByFile)
{
    CSharpInteropClassRenderer renderer = new(classInfo);
    SourceText? source = SourceText.From(renderer.Render(), Encoding.UTF8);
    string outFileName = $"{classInfo.Name}.Interop.cs";
    string outFileDir = Path.GetDirectoryName(fileInfo.Path) ?? throw new InvalidOperationException($"Provided path {fileInfo.Path} has no directory");
    File.WriteAllText(Path.Combine(outFileDir, outFileName), source.ToString());
}

string typescriptFileTarget = "C:\\Users\\marcd\\source\\repos\\DotNetWasmReact\\DotnetWasmTypescript.InteropGenerator\\index.ts";
IEnumerable<ClassInfo> classInfos = classInfoByFile.Select(c => c.ClassInfo);
TypeScriptTypeMapper typeMapper = new(classInfos);
TypescriptClassNameBuilder classNameBuilder = new(typeMapper);
ModuleInfo moduleInfo = new()
{
    ExportedClasses = classInfos,
    HierarchyInfo = ModuleHierarchyInfo.FromClasses(classInfos, classNameBuilder)
};
TypeScriptRenderer tsRenderer = new(classInfos, moduleInfo, classNameBuilder, typeMapper);
File.WriteAllText(typescriptFileTarget, tsRenderer.Render());


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
