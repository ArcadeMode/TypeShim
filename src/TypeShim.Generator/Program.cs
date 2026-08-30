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
    List<NamedTypeInfo> namedTypeInfos = [.. symbolExtractor.ExtractAllExportedSymbols()
        .Select(symbol => new NamedTypeInfoBuilder(symbol, typeInfoCache).Build())
        .OfType<NamedTypeInfo>()]; // drops nulls (non-projected symbols)

    Task csIo = GenerateCSharpInteropCode(parsedArgs, namedTypeInfos);
    Task tsIo = GenerateTypeScriptInteropCode(parsedArgs, namedTypeInfos);
    await Task.WhenAll(csIo, tsIo);
}
catch (TypeShimException ex) // known exceptions warrant only an error message
{
    Console.Error.WriteLine($"TypeShim received invalid input, no code was generated. {ex.GetType().Name} {ex.Message}");
    Environment.Exit(0);
}
// End of main program

static Task GenerateCSharpInteropCode(ProgramArguments parsedArgs, List<NamedTypeInfo> namedTypeInfos)
{
    List<InteropTypeInfo> resolvedTypes = [];
    JSObjectMethodResolver methodResolver = new(resolvedTypes);
    List<Task> ioTasks = new(namedTypeInfos.Count + 1);
    foreach(NamedTypeInfo namedType in namedTypeInfos)
    {
        if (namedType is not ClassInfo classInfo) continue; // enums produce no C# interop
        RenderContext ctx = new(classInfo, namedTypeInfos, RenderOptions.CSharp);
        new CSharpInteropClassRenderer(classInfo, ctx, methodResolver).Render();
        ioTasks.Add(File.WriteAllTextAsync(Path.Combine(parsedArgs.CsOutputDir, $"{classInfo.Name}.g.cs"), ctx.ToString()));
    }
    RenderContext jsObjRenderCtx = new(null, namedTypeInfos, RenderOptions.CSharp);
    new JSObjectExtensionsRenderer(jsObjRenderCtx, resolvedTypes).Render();
    ioTasks.Add(File.WriteAllTextAsync(Path.Combine(parsedArgs.CsOutputDir, "JSObjectExtensions.g.cs"), jsObjRenderCtx.ToString()));
    return Task.WhenAll(ioTasks);
}

static Task GenerateTypeScriptInteropCode(ProgramArguments parsedArgs, List<NamedTypeInfo> namedTypeInfos)
{
    List<ClassInfo> classInfos = [.. namedTypeInfos.OfType<ClassInfo>()];
    ModuleInfo moduleInfo = new()
    {
        ExportedClasses = classInfos,
        HierarchyInfo = ModuleHierarchyInfo.FromClasses(classInfos)
    };
    TypeScriptRenderer tsRenderer = new(namedTypeInfos, moduleInfo);
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