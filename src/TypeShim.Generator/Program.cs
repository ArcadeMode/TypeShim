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

    Task csIo = GenerateCSharpInteropCode(parsedArgs, classInfos);
    Task tsIo = GenerateTypeScriptInteropCode(parsedArgs, classInfos);
    await Task.WhenAll(csIo, tsIo);
}
catch (TypeShimException ex) // known exceptions warrant only an error message
{
    Console.Error.WriteLine($"TypeShim received invalid input, no code was generated. {ex.GetType().Name} {ex.Message}");
    Environment.Exit(0);
}
// End of main program

static Task GenerateCSharpInteropCode(ProgramArguments parsedArgs, List<ClassInfo> classInfos)
{
    List<InteropTypeInfo> resolvedTypes = [];
    JSObjectMethodResolver methodResolver = new(resolvedTypes);
    List<Task> ioTasks = new(classInfos.Count + 1);
    foreach(ClassInfo classInfo in classInfos)
    {
        RenderContext ctx = new(classInfo, classInfos, RenderOptions.CSharp);
        new CSharpInteropClassRenderer(classInfo, ctx, methodResolver).Render();
        ioTasks.Add(File.WriteAllTextAsync(Path.Combine(parsedArgs.CsOutputDir, $"{classInfo.Name}.g.cs"), ctx.ToString()));
    }
    RenderContext jsObjRenderCtx = new(null, classInfos, RenderOptions.CSharp);
    new JSObjectExtensionsRenderer(jsObjRenderCtx, resolvedTypes).Render();
    ioTasks.Add(File.WriteAllTextAsync(Path.Combine(parsedArgs.CsOutputDir, "JSObjectExtensions.g.cs"), jsObjRenderCtx.ToString()));
    return Task.WhenAll(ioTasks);
}

static Task GenerateTypeScriptInteropCode(ProgramArguments parsedArgs, List<ClassInfo> classInfos)
{
    ModuleInfo moduleInfo = new()
    {
        ExportedClasses = classInfos,
        HierarchyInfo = ModuleHierarchyInfo.FromClasses(classInfos)
    };
    TypeScriptRenderer tsRenderer = new(classInfos, moduleInfo);
    return WriteFile(tsRenderer.Render());

    async Task WriteFile(List<RenderContext> ctxs)
    {
        using FileStream fs = new(parsedArgs.TsOutputFilePath, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter tsWriter = new(fs, Encoding.UTF8, 16 * 1024);
        foreach (RenderContext ctx in ctxs)
        {
            await tsWriter.WriteLineAsync(ctx.ToString());
        }
        tsWriter.Flush();
        tsWriter.Close();
    }
}