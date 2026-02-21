using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeShim.Shared;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.CSharp;

internal sealed class JSMarshalAsAttributeRenderer(RenderContext ctx)
{
    internal void RenderJSExportAttribute()
    {
        ctx.Append("[JSExport]");
    }
    
    internal void RenderJSImportAttribute(string method)
    {
        ctx.Append("[JSImport(\"").Append(method).Append("\", \"@typeshim\")]");
    }

    internal void RenderReturnAttribute(TypeSyntax jsTypeSyntax)
    {
        ctx.Append("[return: ");
        RenderAttributeListWithJSMarshalAs(jsTypeSyntax);
        ctx.Append("]");
    }

    internal void RenderParameterAttribute(TypeSyntax jsTypeSyntax)
    {
        ctx.Append("[");
        RenderAttributeListWithJSMarshalAs(jsTypeSyntax);
        ctx.Append("]");
    }

    private void RenderAttributeListWithJSMarshalAs(TypeSyntax jsTypeSyntax)
    {
        ctx.Append("JSMarshalAs<").Append(jsTypeSyntax.ToString()).Append(">");
    }
}