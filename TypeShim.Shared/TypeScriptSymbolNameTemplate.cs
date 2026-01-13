
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeShim.Shared;

internal sealed class TypeScriptSymbolNameTemplate
{
    internal required string Template { get; init; }
    internal required TypeScriptSymbolNameTemplate? InnerTemplate { get; init; }
    
    private const string InnerPlaceholder = "{INNER_PLACEHOLDER}";
    private const string SuffixPlaceholder = "{SUFFIX_PLACEHOLDER}";

    internal string Render(string suffix = "")
    {
        string template = Template;
        if (InnerTemplate is not null)
        {
            string inner = InnerTemplate.Render(suffix);
            template = template.Replace(InnerPlaceholder, inner);
        }

        return template.Replace(SuffixPlaceholder, suffix);
    }

    internal static TypeScriptSymbolNameTemplate ForUserType(string originalTypeSyntax)
    {
        return new TypeScriptSymbolNameTemplate
        {
            Template = $"{originalTypeSyntax}{SuffixPlaceholder}",
            InnerTemplate = null
        };
    }

    internal static TypeScriptSymbolNameTemplate ForSimpleType(string typeName)
    {
        return new TypeScriptSymbolNameTemplate
        {
            Template = typeName,
            InnerTemplate = null
        };
    }

    internal static TypeScriptSymbolNameTemplate ForArrayType(TypeScriptSymbolNameTemplate innerTemplate)
    {
        return new TypeScriptSymbolNameTemplate
        {
            Template = $"Array<{InnerPlaceholder}>",
            InnerTemplate = innerTemplate
        };
    }

    internal static TypeScriptSymbolNameTemplate ForPromiseType(TypeScriptSymbolNameTemplate? innerTemplate)
    {
        return new TypeScriptSymbolNameTemplate
        {
            Template = innerTemplate != null ? $"Promise<{InnerPlaceholder}>" : "Promise<void>",
            InnerTemplate = innerTemplate
        };
    }

    internal static TypeScriptSymbolNameTemplate ForNullableType(TypeScriptSymbolNameTemplate innerTemplate)
    {
        return new TypeScriptSymbolNameTemplate
        {
            Template = $"{InnerPlaceholder} | null",
            InnerTemplate = innerTemplate
        };
    }
}
