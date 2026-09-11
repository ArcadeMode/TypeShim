using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.CSharp;

internal sealed class JSObjectExtensionsRenderer(RenderContext _ctx, IEnumerable<InteropTypeInfo> targetTypeInfos)
{
    public void Render()
    {
        _ctx.AppendLine("#nullable enable")
            .AppendLine("// JSImports for the type marshalling process")
            .AppendLine("using System;")
            .AppendLine("using System.Runtime.InteropServices.JavaScript;")
            .AppendLine("using System.Threading.Tasks;");

        JSObjectExtensionInfo[] extensionInfos = [.. targetTypeInfos
            .Select(typeInfo => new JSObjectExtensionInfo(typeInfo))
            .DistinctBy(extInfo => extInfo.Name)];

        _ctx.AppendLine("public static class JSObjectExtensions")
            .AppendLine("{");
        using (_ctx.Indent())
        {
            bool isFirst = true;
            foreach (JSObjectExtensionInfo typeInfo in extensionInfos)
            {
                if (!isFirst) _ctx.AppendLine();
                RenderExtensionMethodForType(typeInfo);
                isFirst = false;
            }
        }
        _ctx.AppendLine("}");
        _ctx.AppendLine();

        _ctx.Append("public static partial class ").AppendLine(RenderConstants.MarshallPropertyAsClass)
            .AppendLine("{");
        using (_ctx.Indent())
        {
            bool isFirst = true;
            foreach (JSObjectExtensionInfo typeInfo in extensionInfos)
            {
                if (!isFirst) _ctx.AppendLine();
                RenderMarshallerMethodForType(typeInfo);
                isFirst = false;
            }
        }
        _ctx.AppendLine("}");
    }

    private void RenderExtensionMethodForType(JSObjectExtensionInfo extensionInfo)
    {
        InteropTypeInfo type = extensionInfo.TypeInfo;

        _ctx.Append("public static ").Append(type.CSharpInteropTypeSyntax).Append(' ')
            .Append(extensionInfo.GetExtensionMethodName())
            .AppendLine("(this JSObject jsObject, string propertyName)");
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            _ctx.Append("return ").Append(RenderConstants.MarshallPropertyAsClass).Append('.')
                .Append(extensionInfo.GetMarshallerMethodName()).Append("(jsObject, propertyName)");
            if (RequiresNonNullableCoalesce(type))
            {
                _ctx.Append(" ?? throw new InvalidOperationException($\"Marshalling value for property '{propertyName}' yielded unexpected null value, expected non-nullable '")
                    .Append(type.CSharpInteropTypeSyntax).Append("'\")");
            }
            _ctx.AppendLine(";");
        }
        _ctx.AppendLine("}");
    }

    private void RenderMarshallerMethodForType(JSObjectExtensionInfo extensionInfo)
    {
        InteropTypeInfo type = extensionInfo.TypeInfo;

        JSMarshalAsAttributeRenderer attributeRenderer = new(_ctx);
        attributeRenderer.RenderJSImportAttribute("unwrapProperty");
        _ctx.AppendLine();
        attributeRenderer.RenderReturnAttribute(type.JSTypeSyntax);
        _ctx.AppendLine();
        _ctx.Append("public static partial ").Append(type.CSharpInteropTypeSyntax);
        if (RequiresNonNullableCoalesce(type))
        {
            _ctx.Append('?');
        }
        _ctx.Append(' ').Append(extensionInfo.GetMarshallerMethodName()).Append('(');
        attributeRenderer.RenderParameterAttribute(SyntaxFactory.ParseTypeName("JSType.Object"));
        _ctx.Append(' ').Append(InteropTypeInfo.JSObjectTypeInfo.CSharpInteropTypeSyntax).Append(" obj")
            .Append(", ");
        attributeRenderer.RenderParameterAttribute(SyntaxFactory.ParseTypeName("JSType.String"));
        _ctx.Append(" string propertyName")
            .AppendLine(");");
    }

    private static bool RequiresNonNullableCoalesce(InteropTypeInfo type)
        => !type.IsNullableType && type.IsReferenceInteropType;
}
