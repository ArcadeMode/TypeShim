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

internal record JSObjectExtensionInfo(InteropTypeInfo TypeInfo)
{
    internal string Name = string.Join("", GetManagedTypeListForType(TypeInfo));
        
    internal string GetMarshalAsMethodName()
    {
        return $"MarshalAs{Name}";
    }

    internal string GetGetPropertyAsMethodName()
    {
        return $"GetPropertyAs{Name}Nullable";
    }

    private static IEnumerable<KnownManagedType> GetManagedTypeListForType(InteropTypeInfo typeInfo)
    {
        IEnumerable<KnownManagedType> managedTypes = [];
        BuildManagedTypeEnumerableRecursive();
        return managedTypes;

        void BuildManagedTypeEnumerableRecursive()
        {
            if (typeInfo.IsDelegateType() && typeInfo.ArgumentInfo is DelegateArgumentInfo delegateArgInfo)
            {
                foreach (InteropTypeInfo paramType in delegateArgInfo.ParameterTypes)
                {
                    managedTypes = managedTypes.Concat(GetManagedTypeListForType(paramType));
                }
                managedTypes = managedTypes.Concat(GetManagedTypeListForType(delegateArgInfo.ReturnType));
            }
            else if (typeInfo.TypeArgument != null)
            {
                managedTypes = managedTypes.Concat(GetManagedTypeListForType(typeInfo.TypeArgument));
            }
            KnownManagedType managedType = typeInfo is { ManagedType: KnownManagedType.Object, IsTSExport: true }
                ? KnownManagedType.JSObject
                : typeInfo.ManagedType;
            managedTypes = managedTypes.Append(managedType);
        }
    }
}

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

        _ctx.Append("public static ").Append(GetTypeSyntax(extensionInfo.TypeInfo)).Append("? ");
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
        _ctx.AppendLine(attributeRenderer.RenderReturnAttribute(GetJSTypeSyntax(extensionInfo.TypeInfo)).NormalizeWhitespace().ToString());
        _ctx.Append("public static partial ").Append(GetTypeSyntax(extensionInfo.TypeInfo)).Append(' ');
        marshalAsMethodNameRenderer.Render();
        _ctx.Append('(')
            .Append(attributeRenderer.RenderParameterAttribute(SyntaxFactory.ParseTypeName("JSType.Object")).NormalizeWhitespace()).Append(' ').Append(InteropTypeInfo.JSObjectTypeInfo.CSharpInteropTypeSyntax).Append(" obj")
            .Append(", ")
            .Append(attributeRenderer.RenderParameterAttribute(SyntaxFactory.ParseTypeName("JSType.String")).NormalizeWhitespace()).Append(" string propertyName")
            .AppendLine(");");

        static TypeSyntax GetJSTypeSyntax(InteropTypeInfo typeInfo)
        {
            return typeInfo switch
            {
                { ManagedType: KnownManagedType.Object, IsTSExport: true } => InteropTypeInfo.JSObjectTypeInfo.JSTypeSyntax,
                { ManagedType: KnownManagedType.Task } valueTypeInfo => valueTypeInfo.TypeArgument switch
                {
                    { ManagedType: KnownManagedType.Object, IsTSExport: true }
                    or { ManagedType: KnownManagedType.Nullable, TypeArgument: { ManagedType: KnownManagedType.Object, IsTSExport: true } } => SyntaxFactory.ParseTypeName("JSType.Promise<JSType.Object>"),
                    _ => typeInfo.JSTypeSyntax,
                },
                { ManagedType: KnownManagedType.Array } elementTypeInfo => elementTypeInfo.TypeArgument switch
                {
                    { ManagedType: KnownManagedType.Object, IsTSExport: true }
                    or { ManagedType: KnownManagedType.Nullable, TypeArgument: { ManagedType: KnownManagedType.Object, IsTSExport: true } } => SyntaxFactory.ParseTypeName("JSType.Array<JSType.Object>"),
                    _ => typeInfo.JSTypeSyntax,
                },
                _ => typeInfo.JSTypeSyntax,
            };
        }

        static TypeSyntax GetTypeSyntax(InteropTypeInfo typeInfo)
        {
            return typeInfo switch
            {
                { ManagedType: KnownManagedType.Object, IsTSExport: true } => InteropTypeInfo.JSObjectTypeInfo.CSharpTypeSyntax,
                { ManagedType: KnownManagedType.Task } valueTypeInfo => valueTypeInfo.TypeArgument switch
                {
                    { ManagedType: KnownManagedType.Object, IsTSExport: true } => GetTaskTypeSyntax(InteropTypeInfo.JSObjectTypeInfo.CSharpTypeSyntax),
                    { ManagedType: KnownManagedType.Nullable, TypeArgument: { ManagedType: KnownManagedType.Object, IsTSExport: true } } => GetTaskTypeSyntax(SyntaxFactory.NullableType(InteropTypeInfo.JSObjectTypeInfo.CSharpTypeSyntax)),
                    _ => typeInfo.CSharpInteropTypeSyntax,
                },
                { ManagedType: KnownManagedType.Array } elementTypeInfo => elementTypeInfo.TypeArgument switch
                {
                    { ManagedType: KnownManagedType.Object, IsTSExport: true } => GetArrayTypeSyntax(InteropTypeInfo.JSObjectTypeInfo.CSharpTypeSyntax),
                    { ManagedType: KnownManagedType.Nullable, TypeArgument: { ManagedType: KnownManagedType.Object, IsTSExport: true } } => GetArrayTypeSyntax(SyntaxFactory.NullableType(InteropTypeInfo.JSObjectTypeInfo.CSharpTypeSyntax)),
                    _ => typeInfo.CSharpInteropTypeSyntax,
                },
                _ => typeInfo.CSharpInteropTypeSyntax,
            };

            static TypeSyntax GetArrayTypeSyntax(TypeSyntax elementTypeSyntax)
            {
                return SyntaxFactory.ArrayType(elementTypeSyntax, SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression()))));
            }

            static TypeSyntax GetTaskTypeSyntax(TypeSyntax valueTypeSyntax)
            {
                return SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"), SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(valueTypeSyntax)));
            }
        }
    }
}
