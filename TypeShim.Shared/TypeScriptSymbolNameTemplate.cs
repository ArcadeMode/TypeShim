using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace TypeShim.Shared;

internal sealed record TypeScriptFunctionParameterTemplate(string Name, TypeScriptSymbolNameTemplate TypeTemplate);

internal sealed class TypeScriptSymbolNameTemplate
{
    internal string Template { get; init; } = null!;
    internal Dictionary<string, InteropTypeInfo> InnerTypes { get; init; } = []; 
    
    internal const string SuffixPlaceholder = "{SUFFIX_PLACEHOLDER}";

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
    
    internal static TypeScriptSymbolNameTemplate ForDelegateType(DelegateArgumentInfo argumentInfo)
    {
        Dictionary<string, InteropTypeInfo> paramTypeDict = [.. argumentInfo.ParameterTypes.Select((typeInfo, i) => new KeyValuePair<string, InteropTypeInfo>($"{{TArg{i}}}", typeInfo))];
        KeyValuePair<string, InteropTypeInfo> returnTypeKvp = new("{TReturn}", argumentInfo.ReturnType);

        StringBuilder templateBuilder = new();
        templateBuilder.Append('(');
        int i = 0;
        foreach (KeyValuePair<string, InteropTypeInfo> typeInfo in paramTypeDict)
        {
            if (i > 0) templateBuilder.Append(", ");
            templateBuilder.Append(typeInfo.Key).Append(": ").Append(typeInfo.Value);
            i++;
        }
        templateBuilder.Append(") => ").Append(returnTypeKvp.Key);
        return new TypeScriptSymbolNameTemplate
        {
            Template = templateBuilder.ToString(),
            InnerTypes = [..paramTypeDict, returnTypeKvp]
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