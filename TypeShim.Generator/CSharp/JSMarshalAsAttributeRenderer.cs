using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeShim.Core;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.CSharp;

internal sealed class JSMarshalAsAttributeRenderer(InteropTypeInfo interopTypeInfo)
{
    internal AttributeListSyntax RenderJSExportAttribute()
    {
        return SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName("JSExport")
                )
            )
        );
    }

    internal AttributeListSyntax RenderReturnAttribute()
    {
        return RenderAttributeListWithJSMarshalAs().WithTarget( // 'return:'
            SyntaxFactory.AttributeTargetSpecifier(
                SyntaxFactory.Token(SyntaxKind.ReturnKeyword)
            )
        );
    }

    internal AttributeListSyntax RenderParameterAttribute()
    {
        return RenderAttributeListWithJSMarshalAs();
    }

    private AttributeListSyntax RenderAttributeListWithJSMarshalAs()
    {
        TypeArgumentListSyntax marshalAsTypeArgument = SyntaxFactory.TypeArgumentList(
            SyntaxFactory.SingletonSeparatedList(interopTypeInfo.JSTypeSyntax)
        );

        AttributeSyntax marshalAsAttribute = SyntaxFactory.Attribute(SyntaxFactory.GenericName("JSMarshalAs").WithTypeArgumentList(marshalAsTypeArgument));
        AttributeListSyntax attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(marshalAsAttribute));
        return attributeList;
    }
}