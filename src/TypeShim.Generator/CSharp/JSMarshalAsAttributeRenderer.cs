using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeShim.Shared;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.CSharp;

internal sealed class JSMarshalAsAttributeRenderer()
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
    
    internal AttributeListSyntax RenderJSImportAttribute(string method)
    {
        return SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName("JSImport"),
                    SyntaxFactory.AttributeArgumentList([
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.ParseExpression($"\"{method}\"")
                        ),
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.ParseExpression("\"@typeshim\"")
                        )
                    ])
                )
            )
        );
    }

    internal AttributeListSyntax RenderReturnAttribute(TypeSyntax jsTypeSyntax)
    {
        return RenderAttributeListWithJSMarshalAs(jsTypeSyntax).WithTarget( // 'return:'
            SyntaxFactory.AttributeTargetSpecifier(
                SyntaxFactory.Token(SyntaxKind.ReturnKeyword)
            )
        );
    }

    internal AttributeListSyntax RenderParameterAttribute(TypeSyntax jsTypeSyntax)
    {
        return RenderAttributeListWithJSMarshalAs(jsTypeSyntax);
    }

    private AttributeListSyntax RenderAttributeListWithJSMarshalAs(TypeSyntax jsTypeSyntax)
    {
        TypeArgumentListSyntax marshalAsTypeArgument = SyntaxFactory.TypeArgumentList(
            SyntaxFactory.SingletonSeparatedList(jsTypeSyntax)
        );

        AttributeSyntax marshalAsAttribute = SyntaxFactory.Attribute(SyntaxFactory.GenericName("JSMarshalAs").WithTypeArgumentList(marshalAsTypeArgument));
        AttributeListSyntax attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(marshalAsAttribute));
        return attributeList;
    }
}