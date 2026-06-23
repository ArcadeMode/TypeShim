using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeShim.Generator;

internal static class AttributeNameMatcher
{
    // [TSExport], [TSExportAttribute], [TypeShim.TSExport], [global::TSExport]
    internal static bool IsAttributeName(NameSyntax nameSyntax, string attributeName)
    {
        string name = nameSyntax switch
        {
            IdentifierNameSyntax id => id.Identifier.ValueText,
            QualifiedNameSyntax q => q.Right.Identifier.ValueText,
            AliasQualifiedNameSyntax a => a.Name.Identifier.ValueText,
            _ => nameSyntax.ToString()
        };

        return string.Equals(name, attributeName, StringComparison.Ordinal)
            || string.Equals(name, attributeName + "Attribute", StringComparison.Ordinal);
    }
}
