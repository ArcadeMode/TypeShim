using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using TypeShim;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;

if (args.Length != 3)
{
    Console.Error.WriteLine("TypeShim usage: <csFilePaths> <csOutputDir> <tsOutputFilePath>");
    Environment.Exit(1);
}

string[] csFilePaths = args[0].Split(';');
if (csFilePaths.Length == 0)
{
    Console.Error.WriteLine("No .cs file paths provided");
    Environment.Exit(1);
}

string csOutputDir = args[1];
try
{
    csOutputDir = Path.GetFullPath(csOutputDir);
    if (!Directory.Exists(csOutputDir))
    {
        Directory.CreateDirectory(csOutputDir);
    }
} 
catch (Exception)
{
    Console.Error.WriteLine($"Invalid output directory provided '{csOutputDir}'");
    Environment.Exit(1);
}

string tsOutputFilePath = args[2];
if (string.IsNullOrWhiteSpace(tsOutputFilePath))
{
    Console.Error.WriteLine($"Invalid TypeScript output file path provided '{tsOutputFilePath}'");
    Environment.Exit(1);
}
try
{
    tsOutputFilePath = Path.GetFullPath(tsOutputFilePath);
    string tsOutputFileDir = Path.GetDirectoryName(tsOutputFilePath) ?? throw new InvalidOperationException($"Provided path {tsOutputFilePath} has no directory");
    if (!Directory.Exists(tsOutputFileDir))
    {
        Directory.CreateDirectory(tsOutputFileDir);
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Invalid output directory provided '{tsOutputFilePath}'. Error {ex.Message}");
    Environment.Exit(1);
}


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
    //string outFileDir = Path.GetDirectoryName(fileInfo.Path) ?? throw new InvalidOperationException($"Provided path {fileInfo.Path} has no directory");
    File.WriteAllText(Path.Combine(csOutputDir, outFileName), source.ToString());
}

IEnumerable<ClassInfo> classInfos = classInfoByFile.Select(c => c.ClassInfo);
TypeScriptTypeMapper typeMapper = new(classInfos);
TypescriptClassNameBuilder classNameBuilder = new(typeMapper);
ModuleInfo moduleInfo = new()
{
    ExportedClasses = classInfos,
    HierarchyInfo = ModuleHierarchyInfo.FromClasses(classInfos, classNameBuilder)
};
TypeScriptRenderer tsRenderer = new(classInfos, moduleInfo, classNameBuilder, typeMapper);
File.WriteAllText(tsOutputFilePath, tsRenderer.Render());


static IEnumerable<INamedTypeSymbol> FindLabelledClassSymbols(SemanticModel semanticModel, SyntaxNode root)
{
    foreach (var cls in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
    {
        if(semanticModel.GetDeclaredSymbol(cls) is not INamedTypeSymbol symbol)
        {
            continue;
        }
        
        // Example: List attributes
        if (symbol.GetAttributes().Any(attributeData => attributeData.AttributeClass?.Name is "TsExportAttribute" or "TsExport"))
        {
            Console.WriteLine($"TsExport: {symbol.ToDisplayString()}");
            yield return symbol;
        }
    }
}
