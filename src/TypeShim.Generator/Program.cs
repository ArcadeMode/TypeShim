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
    List<InteropTypeInfo> resolvedTypes = [];
    JSObjectMethodResolver methodResolver = new(resolvedTypes);

    foreach (ClassInfo classInfo in classInfos)
    {
        RenderContext renderContext = new(classInfo, classInfos, RenderOptions.CSharp);
        string outFileName = $"{classInfo.Name}.Interop.g.cs";
        File.WriteAllText(Path.Combine(parsedArgs.CsOutputDir, outFileName), new CSharpInteropClassRenderer(classInfo, renderContext, methodResolver).Render());
    }

    RenderContext jsObjRenderCtx = new(null, classInfos, RenderOptions.CSharp);
    new JSObjectExtensionsRenderer(jsObjRenderCtx, resolvedTypes).Render();
    File.WriteAllText(Path.Combine(parsedArgs.CsOutputDir, "JSObjectExtensions.g.cs"), jsObjRenderCtx.ToString());
}

static void GenerateTypeScriptInteropCode(ProgramArguments parsedArgs, List<ClassInfo> classInfos)
{
    ModuleInfo moduleInfo = new()
    {
        ExportedClasses = classInfos,
        HierarchyInfo = ModuleHierarchyInfo.FromClasses(classInfos)
    };
    TypeScriptRenderer tsRenderer = new(classInfos, moduleInfo);
    using FileStream fs = new(parsedArgs.TsOutputFilePath, FileMode.OpenOrCreate, FileAccess.Write);
    StreamWriter tsWriter = new(fs, Encoding.UTF8, 16 * 1024);
    tsRenderer.Render(tsWriter);
    tsWriter.Flush();
    tsWriter.Close();
}