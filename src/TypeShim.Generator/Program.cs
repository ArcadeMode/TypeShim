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
    SymbolExtractor symbolExtractor = new(parsedArgs.CsFileInfos, parsedArgs.RuntimePackRefDir);
    InteropTypeInfoCache typeInfoCache = new();
    List<ClassInfo> classInfos = [.. symbolExtractor.ExtractAllExportedSymbols()
        .Select(classSymbol => new ClassInfoBuilder(classSymbol, typeInfoCache).Build())
        .Where(ci => ci.Methods.Any() || ci.Properties.Any())]; // dont bother with empty classes

    GenerateTypeScriptInteropCode(parsedArgs, classInfos);
    GenerateCSharpInteropCode(parsedArgs, classInfos);
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

    RenderContext[] classCtxs = new RenderContext[classInfos.Count];
    ParallelOptions options = new()
    {
        MaxDegreeOfParallelism = Math.Min(4, Environment.ProcessorCount)
    };
    Parallel.For(0, classInfos.Count, options, i =>
    {
        classCtxs[i] = new(classInfos[i], classInfos, RenderOptions.CSharp);
        new CSharpInteropClassRenderer(classInfos[i], classCtxs[i], methodResolver).Render();
    });

    foreach(RenderContext ctx in classCtxs)
    {
        File.WriteAllText(Path.Combine(parsedArgs.CsOutputDir, $"{ctx.Class.Name}.Interop.g.cs"), ctx.ToString());
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