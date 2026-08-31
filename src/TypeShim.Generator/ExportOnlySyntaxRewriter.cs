using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeShim.Generator;

public sealed class ExportOnlySyntaxRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (HasTSExportAttribute(node.AttributeLists))
        {
            // TSExport classes are kept whole - downstream parsing already filters by accessibility/kind.
            return node;
        }

        return RewriteForJSExport(node);
    }

    public override SyntaxNode? VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        return HasTSExportAttribute(node.AttributeLists) ? node : null;
    }

    private static bool HasTSExportAttribute(SyntaxList<AttributeListSyntax> attributeLists)
    {
        foreach (AttributeListSyntax attributeList in attributeLists)
        {
            foreach (AttributeSyntax attribute in attributeList.Attributes)
            {
                if (AttributeNameMatcher.IsAttributeName(attribute.Name, "TSExport"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static ClassDeclarationSyntax? RewriteForJSExport(ClassDeclarationSyntax node)
    {
        List<MemberDeclarationSyntax> kept = new();
        foreach (MemberDeclarationSyntax member in node.Members)
        {
            if (HasJSExportAttribute(member))
            {
                kept.Add(member);
            }
        }

        if (kept.Count == 0)
        {
            return null;
        }
        return node.WithMembers(SyntaxFactory.List(kept));
    }

    private static bool HasJSExportAttribute(MemberDeclarationSyntax member)
    {
        foreach (AttributeListSyntax attributeList in member.AttributeLists)
        {
            foreach (AttributeSyntax attribute in attributeList.Attributes)
            {
                if (AttributeNameMatcher.IsAttributeName(attribute.Name, "JSExport"))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
