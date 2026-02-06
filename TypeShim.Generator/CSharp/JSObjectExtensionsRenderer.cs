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
            .AppendLine("using System.Threading.Tasks;")
            .AppendLine("public static partial class JSObjectExtensions")
            .AppendLine("{");
        using (_ctx.Indent())
        {
            JSObjectExtensionInfo[] extensionInfos = [.. targetTypeInfos
                .Select(typeInfo => new JSObjectExtensionInfo(typeInfo))
                .DistinctBy(extInfo => extInfo.Name)];
            HashSet<string> processedTypes = [];
            foreach (JSObjectExtensionInfo typeInfo in extensionInfos)
            {
                RenderExtensionMethodForType(typeInfo);
            }
        }
        _ctx.AppendLine("}");
    }

    private void RenderExtensionMethodForType(JSObjectExtensionInfo extensionInfo)
    {
        DeferredExpressionRenderer marshalAsMethodNameRenderer = DeferredExpressionRenderer.From(() =>
        {
            _ctx.Append("MarshalAs").Append(extensionInfo.Name);
        });
        DeferredExpressionRenderer getPropertyAsMethodNameRenderer = DeferredExpressionRenderer.From(() =>
        {
            _ctx.Append("GetPropertyAs").Append(extensionInfo.Name).Append("Nullable");
        });

        _ctx.Append("public static ").Append(extensionInfo.TypeInfo.CSharpInteropTypeSyntax).Append("? ");
        getPropertyAsMethodNameRenderer.Render();
        _ctx.AppendLine("(this JSObject jsObject, string propertyName)");
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            _ctx.Append("return jsObject.HasProperty(propertyName) ? ");
            marshalAsMethodNameRenderer.Render();
            _ctx.AppendLine("(jsObject, propertyName) : null;");
        }
        _ctx.AppendLine("}");

        JSMarshalAsAttributeRenderer attributeRenderer = new();
        _ctx.AppendLine(attributeRenderer.RenderJSImportAttribute("unwrapProperty").NormalizeWhitespace().ToString());
        _ctx.AppendLine(attributeRenderer.RenderReturnAttribute(extensionInfo.TypeInfo.JSTypeSyntax).NormalizeWhitespace().ToString());
        _ctx.Append("public static partial ").Append(extensionInfo.TypeInfo.CSharpInteropTypeSyntax).Append(' ');
        marshalAsMethodNameRenderer.Render();
        _ctx.Append('(')
            .Append(attributeRenderer.RenderParameterAttribute(SyntaxFactory.ParseTypeName("JSType.Object")).NormalizeWhitespace()).Append(' ').Append(InteropTypeInfo.JSObjectTypeInfo.CSharpInteropTypeSyntax).Append(" obj")
            .Append(", ")
            .Append(attributeRenderer.RenderParameterAttribute(SyntaxFactory.ParseTypeName("JSType.String")).NormalizeWhitespace()).Append(" string propertyName")
            .AppendLine(");");
    }
}
