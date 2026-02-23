using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace TypeShim.Generator;

public sealed class TSExportOnlySyntaxRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (!HasAttribute(node.AttributeLists, "TSExport")) 
            return null;
        
        return node; // base.VisitClassDeclaration(node); (if nested types ever get supported)
    }

    private static bool HasAttribute(SyntaxList<AttributeListSyntax> attributeLists, string attributeName)
        => attributeLists
            .SelectMany(al => al.Attributes)
            .Any(a => IsAttributeName(a.Name, attributeName));

    private static bool IsAttributeName(NameSyntax nameSyntax, string attributeName)
    {
        // [TSExport], [TSExportAttribute], [TypeShim.TSExport], [global::TSExport]
        var name = nameSyntax switch
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