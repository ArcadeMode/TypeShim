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
            .AppendLine("using System.Threading.Tasks;");

        // raison d'etre: type mapping limitations: https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mapping-limitations
        // 1. JSObject has no means to retrieve arrays beside ByteArray (automapping user classes with an array property type is therefore not possible by default)
        // 2. Nested types cannot be represented on the interop boundary (i.e. Task<int[]>
        // 3. Not all type mappings can be predefined: mostly multi-generics like func/action types, as they will have huge numbers of type param permutations, i.e. 3 param funcs (with simple types only) permute to ~3375 type configurations)

        _ctx.AppendLine("public static partial class JSObjectExtensions")
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
        _ctx.AppendLine("([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string propertyName);");

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

    private const string JSObjectIntExtensionsClass = """
public static partial class JSObjectIntExtensions
{
    public static byte? GetPropertyAsByteNullable(this JSObject jsObject, string propertyName)
    {
        return jsObject.HasProperty(propertyName) ? (byte)jsObject.GetPropertyAsInt32(propertyName) : null;
    }

    public static short? GetPropertyAsInt16Nullable(this JSObject jsObject, string propertyName)
    {
        return jsObject.HasProperty(propertyName) ? (short)jsObject.GetPropertyAsInt32(propertyName) : null;
    }

    public static int? GetPropertyAsInt32Nullable(this JSObject jsObject, string propertyName)
    {
        return jsObject.HasProperty(propertyName) ? jsObject.GetPropertyAsInt32(propertyName) : null;
    }

    public static long? GetPropertyAsInt64Nullable(this JSObject jsObject, string propertyName)
    {
        return jsObject.HasProperty(propertyName) ? (long)jsObject.GetPropertyAsInt32(propertyName) : null;
    }

    public static nint? GetPropertyAsIntPtrNullable(this JSObject jsObject, string propertyName)
    {
        return jsObject.HasProperty(propertyName) ? (nint)jsObject.GetPropertyAsInt32(propertyName) : null;
    }

    public static bool? GetPropertyAsBooleanNullable(this JSObject jsObject, string propertyName)
    {
        return jsObject.HasProperty(propertyName) ? jsObject.GetPropertyAsBoolean(propertyName) : null;
    }

    public static double? GetPropertyAsDoubleNullable(this JSObject jsObject, string propertyName)
    {
        return jsObject.HasProperty(propertyName) ? jsObject.GetPropertyAsDouble(propertyName) : null;
    }

    public static float? GetPropertyAsFloatNullable(this JSObject jsObject, string propertyName)
    {
        return jsObject.HasProperty(propertyName) ? (float)jsObject.GetPropertyAsDouble(propertyName) : null;
    }

    public static char? GetPropertyAsCharNullable(this JSObject jsObject, string propertyName)
    {
        return jsObject.HasProperty(propertyName) ? char.Parse(jsObject.GetPropertyAsString(propertyName)!) : null;
    }

    public static DateTime? GetPropertyAsDateTimeNullable(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsDateTime(value) : null;
    }

    public static DateTimeOffset? GetPropertyAsDateTimeOffsetNullable(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsDateTimeOffset(value) : null;
    }

    public static object? GetPropertyAsObject(this JSObject jsObject, string propertyName)
    {
        return jsObject.HasProperty(propertyName) ? MarshallPropertyAsObject(jsObject, propertyName) : null;
    }

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Date>]
    public static partial DateTime MarshallAsDateTime([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Date>]
    public static partial DateTimeOffset MarshallAsDateTimeOffset([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrapProperty", "@typeshim")]
    [return: JSMarshalAs<JSType.Any>]
    public static partial object MarshallPropertyAsObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string propertyName);
}
        
""";

    private const string JSObjectArrayExtensionsClass = """
public static partial class JSObjectArrayExtensions
{
    public static int[]? GetPropertyAsInt32Array(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsIntArray(value) : null;
    }

    public static double[]? GetPropertyAsDoubleArray(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsDoubleArray(value) : null;
    }

    public static string[]? GetPropertyAsStringArray(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsStringArray(value) : null;
    }

    public static JSObject[]? GetPropertyAsJSObjectArray(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsJSObjectArray(value) : null;
    }

    public static object[]? GetPropertyAsObjectArray(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsObjectArray(value) : null;
    }

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static partial int[] MarshallAsIntArray([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static partial double[] MarshallAsDoubleArray([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Array<JSType.String>>]
    public static partial string[] MarshallAsStringArray([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Array<JSType.Object>>]
    public static partial JSObject[] MarshallAsJSObjectArray([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Array<JSType.Any>>]
    public static partial object[] MarshallAsObjectArray([JSMarshalAs<JSType.Object>] JSObject jsObject);
}

""";
    
    private const string JSObjectTaskExtensionsClass = """
public static partial class JSObjectTaskExtensions
{
    public static Task? GetPropertyAsVoidTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsVoidTask(value) : null;
    }

    public static Task<bool>? GetPropertyAsBooleanTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsBooleanTask(value) : null;
    }

    public static Task<byte>? GetPropertyAsByteTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsByteTask(value) : null;
    }

    public static Task<char>? GetPropertyAsCharTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsCharTask(value) : null;
    }

    public static Task<short>? GetPropertyAsInt16Task(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsInt16Task(value) : null;
    }

    public static Task<int>? GetPropertyAsInt32Task(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsInt32Task(value) : null;
    }

    public static Task<long>? GetPropertyAsInt64Task(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsInt64Task(value) : null;
    }

    public static Task<float>? GetPropertyAsSingleTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsSingleTask(value) : null;
    }

    public static Task<double>? GetPropertyAsDoubleTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsDoubleTask(value) : null;
    }

    public static Task<nint>? GetPropertyAsIntPtrTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsIntPtrTask(value) : null;
    }

    public static Task<DateTime>? GetPropertyAsDateTimeTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsDateTimeTask(value) : null;
    }

    public static Task<DateTimeOffset>? GetPropertyAsDateTimeOffsetTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsDateTimeOffsetTask(value) : null;
    }

    public static Task<Exception>? GetPropertyAsExceptionTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsExceptionTask(value) : null;
    }

    public static Task<JSObject>? GetPropertyAsJSObjectTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsJSObjectTask(value) : null;
    }

    public static Task<string>? GetPropertyAsStringTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsStringTask(value) : null;
    }

    public static Task<object>? GetPropertyAsObjectTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsObjectTask(value) : null;
    }

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Void>>]
    public static partial Task MarshallAsVoidTask([JSMarshalAs<JSType.Object>] JSObject jsObject);
    
    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Boolean>>]
    public static partial Task<bool> MarshallAsBooleanTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<byte> MarshallAsByteTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrapCharPromise", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.String>>]
    public static partial Task<char> MarshallAsCharTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<short> MarshallAsInt16Task([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<int> MarshallAsInt32Task([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<long> MarshallAsInt64Task([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<float> MarshallAsSingleTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<double> MarshallAsDoubleTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<nint> MarshallAsIntPtrTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Date>>]
    public static partial Task<DateTime> MarshallAsDateTimeTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Date>>]
    public static partial Task<DateTimeOffset> MarshallAsDateTimeOffsetTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Error>>]
    public static partial Task<Exception> MarshallAsExceptionTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Object>>]
    public static partial Task<JSObject> MarshallAsJSObjectTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.String>>]
    public static partial Task<string> MarshallAsStringTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
    public static partial Task<object> MarshallAsObjectTask([JSMarshalAs<JSType.Object>] JSObject jsObject);
}

""";
}
