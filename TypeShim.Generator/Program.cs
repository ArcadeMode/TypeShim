using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using TypeShim.Shared;
using TypeShim.Generator;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;

ProgramArguments parsedArgs = ProgramArguments.Parse(args);

try
{
    SymbolExtractor symbolExtractor = new(parsedArgs.CsFileInfos);
    InteropTypeInfoCache typeInfoCache = new();
    List<ClassInfo> classInfos = [.. symbolExtractor.ExtractAllExportedSymbols()
        .Select(classSymbol => new ClassInfoBuilder(classSymbol, typeInfoCache).Build())
        .Where(ci => ci.Methods.Any() || ci.Properties.Any())]; // dont bother with empty classes

    Task generateTS = Task.Run(() => GenerateTypeScriptInteropCode(parsedArgs, classInfos));
    Task generateCS = Task.Run(() => GenerateCSharpInteropCode(parsedArgs, classInfos));

    await Task.WhenAll(generateTS, generateCS);
} 
catch (TypeShimException ex) // known exceptions warrant only an error message
{
    Console.Error.WriteLine($"TypeShim received invalid input, no code was generated. {ex.GetType().Name} {ex.Message}");
    Environment.Exit(0);
}

// End of main program

static void GenerateCSharpInteropCode(ProgramArguments parsedArgs, List<ClassInfo> classInfos)
{
    foreach (ClassInfo classInfo in classInfos)
    {
        RenderContext renderContext = new(classInfo, classInfos, RenderOptions.CSharp);
        SourceText source = SourceText.From(new CSharpInteropClassRenderer(classInfo, renderContext).Render(), Encoding.UTF8);
        string outFileName = $"{classInfo.Name}.Interop.g.cs";
        File.WriteAllText(Path.Combine(parsedArgs.CsOutputDir, outFileName), source.ToString());
    }

    JSObjectArrayExtensionsRenderer jsObjectArrayExtensionsRenderer = new();
    SourceText jsObjectArrayExtensionsSource = SourceText.From(jsObjectArrayExtensionsRenderer.Render(), Encoding.UTF8);
    File.WriteAllText(Path.Combine(parsedArgs.CsOutputDir, "JSObjectArrayExtensions.g.cs"), jsObjectArrayExtensionsSource.ToString());

    JSObjectTaskExtensionsRenderer jsObjectTaskExtensionsRenderer = new();
    SourceText jsObjectTaskExtensionsSource = SourceText.From(jsObjectTaskExtensionsRenderer.Render(), Encoding.UTF8);
    File.WriteAllText(Path.Combine(parsedArgs.CsOutputDir, "JSObjectTaskExtensions.g.cs"), jsObjectTaskExtensionsSource.ToString());
}

static void GenerateTypeScriptInteropCode(ProgramArguments parsedArgs, List<ClassInfo> classInfos)
{
    TypeScriptTypeMapper typeMapper = new(classInfos);
    TypescriptSymbolNameProvider symbolNameProvider = new(typeMapper);
    ModuleInfo moduleInfo = new()
    {
        ExportedClasses = classInfos,
        HierarchyInfo = ModuleHierarchyInfo.FromClasses(classInfos, symbolNameProvider)
    };
    TypeScriptRenderer tsRenderer = new(classInfos, moduleInfo, symbolNameProvider);
    File.WriteAllText(parsedArgs.TsOutputFilePath, tsRenderer.Render());
}