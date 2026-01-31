
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Collections.Generic;

namespace TypeShim.Shared;

internal sealed record TypeScriptFunctionParameterTemplate(string Name, TypeScriptSymbolNameTemplate TypeTemplate)
{
    internal string Render(string suffix = "")
    {
        return $"{Name}: {TypeTemplate.Render(suffix)}";
    }
}

internal sealed class TypeScriptSymbolNameTemplate
{
    private string Template { get; init; } = null!;
    private Dictionary<string, InteropTypeInfo> InnerTypes { get; init; } = []; 
    
    private const string SuffixPlaceholder = "{SUFFIX_PLACEHOLDER}";

    internal string Render(string suffix = "") // PUT SYMBOLTYPE HERE
    {
        string template = Template;

        foreach (KeyValuePair<string, InteropTypeInfo> kvp in InnerTypes)
        {
            template = template.Replace(kvp.Key, kvp.Value.TypeScriptInteropTypeSyntax.Render(suffix));
        }

        return template.Replace(SuffixPlaceholder, suffix);
    }

    internal static TypeScriptSymbolNameTemplate ForUserType(string originalTypeSyntax)
    {
        return new TypeScriptSymbolNameTemplate
        {
            Template = $"{originalTypeSyntax}{SuffixPlaceholder}",
        };
    }

    internal static TypeScriptSymbolNameTemplate ForSimpleType(string typeName)
    {
        return new TypeScriptSymbolNameTemplate
        {
            Template = typeName,
        };
    }

    internal static TypeScriptSymbolNameTemplate ForArrayType(InteropTypeInfo innerType)
    {
        return new TypeScriptSymbolNameTemplate
        {
            Template = "Array<{TElement}>",
            InnerTypes = { { "{TElement}", innerType } }
        };
    }

    internal static TypeScriptSymbolNameTemplate ForPromiseType(InteropTypeInfo? innerType)
    {
        if (innerType == null)
        {
            return new TypeScriptSymbolNameTemplate
            {
                Template = "Promise<void>",
            };
        }
        return new TypeScriptSymbolNameTemplate
        {
            Template = "Promise<{TValue}>",
            InnerTypes = { { "{TValue}", innerType } }
        };
    }

    internal static TypeScriptSymbolNameTemplate ForNullableType(InteropTypeInfo innerType)
    {
        return new TypeScriptSymbolNameTemplate
        {
            Template = "{TNullableValue} | null",
            InnerTypes = { { "{TNullableValue}", innerType } }
        };
    }
    
    internal static TypeScriptSymbolNameTemplate ForDelegateType(TypeScriptFunctionParameterTemplate[] parameterTemplates, TypeScriptSymbolNameTemplate returnTypeTemplate)
    {
        Dictionary<string, TypeScriptSymbolNameTemplate> paramTypeDict = parameterTemplates.ToDictionary(pt => $"{{T{pt.Name}}}", pt => pt.TypeTemplate);
        KeyValuePair<string, TypeScriptSymbolNameTemplate> returnTypeKvp = new("{TReturn}", returnTypeTemplate);
        return new TypeScriptSymbolNameTemplate
        {
            Template = $"({string.Join(", ", paramTypeDict.Keys)}) => {returnTypeKvp.Key}",
            InnerTemplates = [..paramTypeDict, returnTypeKvp]
        };
    }
}

public static class DictionaryExtensions
{
    // Enables collection expressions for Dictionary<TKey, TValue> like newdict = [..dict, kvp]
    public static Dictionary<TKey, TValue> Add<TKey, TValue>(this Dictionary<TKey, TValue> dict, KeyValuePair<TKey, TValue> keyValuePair) where TKey : notnull
    {
        dict[keyValuePair.Key] = keyValuePair.Value;
        return dict;
    }
}