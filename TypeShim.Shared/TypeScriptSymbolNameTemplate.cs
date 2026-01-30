
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Collections.Generic;

namespace TypeShim.Shared;

internal sealed record TypeScriptFunctionParameterTemplate(string name, TypeScriptSymbolNameTemplate typeTemplate)
{
    internal string Render(string suffix = "")
    {
        return $"{name}: {typeTemplate.Render(suffix)}";
    }
}

internal sealed class TypeScriptSymbolNameTemplate
{
    private string Template { get; init; } = null!;
    //private TypeScriptSymbolNameTemplate? InnerTemplate { get; init; }
    private Dictionary<string, TypeScriptSymbolNameTemplate> InnerTemplates { get; init; } = []; 
    
    private const string InnerPlaceholder = "{INNER_PLACEHOLDER}";
    private const string SuffixPlaceholder = "{SUFFIX_PLACEHOLDER}";

    internal string Render(string suffix = "")
    {
        string template = Template;

        foreach (KeyValuePair<string, TypeScriptSymbolNameTemplate> kvp in InnerTemplates)
        {
            template = template.Replace(kvp.Key, kvp.Value.Render(suffix));
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

    internal static TypeScriptSymbolNameTemplate ForArrayType(TypeScriptSymbolNameTemplate innerTemplate)
    {
        return new TypeScriptSymbolNameTemplate
        {
            Template = "Array<{TElement}>",
            InnerTemplates = { { "{TElement}", innerTemplate } }
        };
    }

    internal static TypeScriptSymbolNameTemplate ForPromiseType(TypeScriptSymbolNameTemplate? innerTemplate)
    {
        if (innerTemplate == null)
        {
            return new TypeScriptSymbolNameTemplate
            {
                Template = "Promise<void>",
                InnerTemplates = []
            };
        }
        return new TypeScriptSymbolNameTemplate
        {
            Template = "Promise<{TValue}>",
            InnerTemplates = { { "{TValue}", innerTemplate } }
        };
    }

    internal static TypeScriptSymbolNameTemplate ForNullableType(TypeScriptSymbolNameTemplate innerTemplate)
    {
        return new TypeScriptSymbolNameTemplate
        {
            Template = "{TNullableValue} | null",
            InnerTemplates = { { "{TNullableValue}", innerTemplate } }
        };
    }
    
    internal static TypeScriptSymbolNameTemplate ForDelegateType(TypeScriptFunctionParameterTemplate[] parameterTemplates, TypeScriptSymbolNameTemplate returnTypeTemplate)
    {
        return new TypeScriptSymbolNameTemplate
        {
            // TODO: restructure class to support function types better (now cannot render suffixes for parameter types)
            Template = $"({string.Join(", ", parameterTemplates.Select(t => t.Render()))}) => {returnTypeTemplate.Render()}",
            InnerTemplates = []
        };
    }
}
