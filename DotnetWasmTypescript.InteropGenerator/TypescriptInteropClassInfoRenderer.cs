using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetWasmTypescript.InteropGenerator;

internal class TypescriptInteropInterfaceRenderer(ClassInfo classInfo, TypeScriptTypeMapper typeMapper)
{
    //TODO: render mirror TypeScript interop class

    // PURPOSE:
    // - provide strongly typed interop interface for a single class instance
    // - recognizable methods matching the original C# class for end user

    private readonly StringBuilder sb = new();

    internal string Render()
    {
        sb.AppendLine($"// Auto-generated TypeScript interop definitions for {classInfo.Namespace}.{classInfo.Name}");
        sb.AppendLine();
        sb.AppendLine($"export interface {classInfo.Name} {{");
        foreach (MethodInfo methodInfo in classInfo.Methods)
        {
            sb.AppendLine($"    {RenderMethodSignature(methodInfo)};");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private string RenderMethodSignature(MethodInfo methodInfo)
    {
        return $"{methodInfo.Name}({RenderMethodParameters(methodInfo)}): {typeMapper.ToTypeScriptType(methodInfo.ReturnKnownType, methodInfo.ReturnCLRTypeSyntax.ToString())}";
        
    }

    private string RenderMethodParameters(MethodInfo methodInfo)
    {
        return string.Join(", ", methodInfo.MethodParameters.Select(p => $"{p.ParameterName}: {typeMapper.ToTypeScriptType(p.KnownType, p.CLRTypeSyntax.ToString())}"));
    }
}

internal class TypescriptModuleInterfaceRenderer(IEnumerable<ClassInfo> classInfos)
{
    //TODO: render interface with all exported interop methods

    // PURPOSE:
    // - provide strongly typed interface of the WebAssembly module's exports
    
    // KEY POINTS:
    // - defines the entire set of exports available from the WebAssembly module
    // - considers namespaces correctly
}

internal class TypescriptModuleImplementationRenderer(IEnumerable<ClassInfo> classInfos)
{
    //TODO: render class that implements all exported interop methods

    // PURPOSE:
    // - first unit (that user calls) to tie together the object references + interop interface + class proxies

    // KEY POINTS:
    // - first line of interop
    // - hidden from user behind interface with module exports
}

internal class TypescriptInterfaceRenderer(ClassInfo classInfo)
{
    //TODO: render the original class's TypeScript interface

    // PURPOSE:
    // - provide strongly typed interface matching the original C# class
    // - hide interop details from the end user >> hide (xxxProxy) behind this interface
}

internal class TypescriptProxyClassInfoRenderer(ClassInfo classInfo)
{
    //TODO: render proxy class implementing the interface rendered by TypescriptInterfaceRenderer

    // PURPOSE:
    // - glue between interop interface for a single class instance, enabling dynamic method invocation

    // CONSTRUCTOR:
    // - constructor takes ref to managedObject (js runtime) to pass as instance parameter to interop calls
    // --- IF the original class has non-static methods
    // - contructor takes ref to exports interface by TypescriptWasmExportsInterfaceClassInfoRenderer
}

internal class TypeScriptTypeMapper(IEnumerable<ClassInfo> classInfos)
{
    private readonly HashSet<string> _customTypeNames = [.. classInfos.Select(ci => ci.Name)];

    public string ToTypeScriptType(KnownManagedType type, string nameHint)
    {
        if (_customTypeNames.Contains(nameHint)) return nameHint;

        return type switch
        {
            KnownManagedType.None => "undefined",
            KnownManagedType.Void => "void",
            KnownManagedType.Boolean => "boolean",
            KnownManagedType.Byte => "number",
            KnownManagedType.Char => "string",
            KnownManagedType.Int16 => "number",
            KnownManagedType.Int32 => "number",
            KnownManagedType.Int64 => "number",
            KnownManagedType.Double => "number",
            KnownManagedType.Single => "number",
            KnownManagedType.IntPtr => "number", // JS doesn't have pointers, typically represented as number
            KnownManagedType.JSObject => "object",
            KnownManagedType.Object => "object",
            KnownManagedType.String => "string",
            KnownManagedType.Exception => "Error",
            KnownManagedType.DateTime => "Date",
            KnownManagedType.DateTimeOffset => "Date",
            KnownManagedType.Nullable => "number | null", // generic fallback, could be more precise
            KnownManagedType.Task => "Promise<any>",  // could be mapped more precisely
            KnownManagedType.Array => "any[]",
            KnownManagedType.ArraySegment => "any[]",
            KnownManagedType.Span => "any[]",
            KnownManagedType.Action => "(() => void)",
            KnownManagedType.Function => "Function",
            KnownManagedType.Unknown => "any",
            _ => "any"
        };
    }
}