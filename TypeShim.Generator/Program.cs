using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using TypeShim.Generator;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;

ProgramArguments parsedArgs = ProgramArguments.Parse(args);

CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation(parsedArgs.CsFileInfos.Select(csFile => csFile.SyntaxTree));

List<ClassInfo> classInfos = [.. parsedArgs.CsFileInfos
    .SelectMany(fileInfo => TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(fileInfo.SyntaxTree), fileInfo.SyntaxTree.GetRoot()))
    .Select(classSymbol => new ClassInfoBuilder(classSymbol).Build())
    .Where(ci => ci.Methods.Any() || ci.Properties.Any())]; // dont bother with empty classes

Task generateTS = Task.Run(() => GenerateTypeScriptInteropCode(parsedArgs, classInfos));
Task generateCS = Task.Run(() => GenerateCSharpInteropCode(parsedArgs, classInfos));

await Task.WhenAll(generateTS, generateCS);
// End of main program

static void GenerateCSharpInteropCode(ProgramArguments parsedArgs, List<ClassInfo> classInfos)
{
    foreach (ClassInfo classInfo in classInfos)
    {
        SourceText source = SourceText.From(new CSharpInteropClassRenderer(classInfo).Render(), Encoding.UTF8);
        string outFileName = $"{classInfo.Name}.Interop.g.cs";
        File.WriteAllText(Path.Combine(parsedArgs.CsOutputDir, outFileName), source.ToString());
    }
}

static void GenerateTypeScriptInteropCode(ProgramArguments parsedArgs, List<ClassInfo> classInfos)
{
    TypeScriptTypeMapper typeMapper = new(classInfos);
    TypescriptClassNameBuilder classNameBuilder = new(typeMapper);
    ModuleInfo moduleInfo = new()
    {
        ExportedClasses = classInfos,
        HierarchyInfo = ModuleHierarchyInfo.FromClasses(classInfos, classNameBuilder)
    };
    TypeScriptRenderer tsRenderer = new(classInfos, moduleInfo, classNameBuilder, typeMapper);
    File.WriteAllText(parsedArgs.TsOutputFilePath, tsRenderer.Render());
}