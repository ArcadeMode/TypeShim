using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeShim.Generator;

public sealed class ExportOnlySyntaxRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (HasTSExportAttribute(node))
        {
            // TSExport classes are kept whole - downstream parsing already filters by accessibility/kind.
            return node;
        }

        return RewriteForJSExport(node);
    }

    private static bool HasTSExportAttribute(ClassDeclarationSyntax node)
    {
        foreach (AttributeListSyntax attributeList in node.AttributeLists)
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
