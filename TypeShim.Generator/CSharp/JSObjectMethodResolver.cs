using TypeShim.Shared;

namespace TypeShim.Generator.CSharp;

internal static class JSObjectMethodResolver
{
    internal static string ResolveJSObjectMethodName(InteropTypeInfo typeInfo)
    {
        return typeInfo.ManagedType switch
        {
            KnownManagedType.Nullable => ResolveJSObjectMethodName(typeInfo.TypeArgument!),
            KnownManagedType.Boolean => "GetPropertyAsBooleanNullable",
            KnownManagedType.Double => "GetPropertyAsDoubleNullable",
            KnownManagedType.Single => "GetPropertyAsFloatNullable",
            KnownManagedType.Char => "GetPropertyAsCharNullable",
            KnownManagedType.String => "GetPropertyAsString",
            KnownManagedType.Byte => "GetPropertyAsByteNullable",
            KnownManagedType.Int16 => "GetPropertyAsInt16Nullable",
            KnownManagedType.Int32 => "GetPropertyAsInt32Nullable",
            KnownManagedType.Int64 => "GetPropertyAsInt64Nullable",
            KnownManagedType.IntPtr => "GetPropertyAsIntPtrNullable",
            KnownManagedType.JSObject => "GetPropertyAsJSObject",
            KnownManagedType.Object when typeInfo.IsTSExport => "GetPropertyAsJSObject", // exported object types have a FromJSObject mapper
            KnownManagedType.Object when !typeInfo.IsTSExport => "GetPropertyAsObject", // non-exports are just casted to their original type.
            KnownManagedType.Array => typeInfo.TypeArgument switch
            {
                { ManagedType: KnownManagedType.Byte } => "GetPropertyAsByteArray",
                { ManagedType: KnownManagedType.Int32 } => "GetPropertyAsInt32Array",
                { ManagedType: KnownManagedType.Double } => "GetPropertyAsDoubleArray",
                { ManagedType: KnownManagedType.String } => "GetPropertyAsStringArray",
                { ManagedType: KnownManagedType.JSObject } => "GetPropertyAsJSObjectArray",
                { ManagedType: KnownManagedType.Object, IsTSExport: true } => "GetPropertyAsJSObjectArray", // exported object types have a FromJSObject mapper
                { ManagedType: KnownManagedType.Object, IsTSExport: false } => "GetPropertyAsObjectArray", // non-exports are just casted to their original type.
                { ManagedType: KnownManagedType.Nullable } elemTypeInfo => elemTypeInfo.TypeArgument switch
                {
                    { ManagedType: KnownManagedType.JSObject } => "GetPropertyAsJSObjectArray",
                    { ManagedType: KnownManagedType.Object, IsTSExport: true } => "GetPropertyAsJSObjectArray", // exported object types have a FromJSObject mapper
                    { ManagedType: KnownManagedType.Object, IsTSExport: false } => "GetPropertyAsObjectArray", // non-exports are just casted to their original type.
                    _ => throw new InvalidOperationException($"Array of nullable type '{elemTypeInfo?.ManagedType}' cannot be marshalled through TypeShim JSObject extensions"),
                },
                _ => throw new InvalidOperationException($"Array of type '{typeInfo.TypeArgument?.ManagedType}' cannot be marshalled through TypeShim JSObject extensions"),
            },
            KnownManagedType.Task => typeInfo.TypeArgument switch
            {
                null or { ManagedType: KnownManagedType.Void } => "GetPropertyAsVoidTask",
                { ManagedType: KnownManagedType.Boolean } => "GetPropertyAsBooleanTask",
                { ManagedType: KnownManagedType.Byte } => "GetPropertyAsByteTask",
                { ManagedType: KnownManagedType.Char } => "GetPropertyAsCharTask",
                { ManagedType: KnownManagedType.Int16 } => "GetPropertyAsInt16Task",
                { ManagedType: KnownManagedType.Int32 } => "GetPropertyAsInt32Task",
                { ManagedType: KnownManagedType.Int64 } => "GetPropertyAsInt64Task",
                { ManagedType: KnownManagedType.Single } => "GetPropertyAsSingleTask",
                { ManagedType: KnownManagedType.Double } => "GetPropertyAsDoubleTask",
                { ManagedType: KnownManagedType.IntPtr } => "GetPropertyAsIntPtrTask",
                { ManagedType: KnownManagedType.DateTime } => "GetPropertyAsDateTimeTask",
                { ManagedType: KnownManagedType.DateTimeOffset } => "GetPropertyAsDateTimeOffsetTask",
                { ManagedType: KnownManagedType.Exception } => "GetPropertyAsExceptionTask",
                { ManagedType: KnownManagedType.String } => "GetPropertyAsStringTask",
                { ManagedType: KnownManagedType.JSObject } => "GetPropertyAsJSObjectTask",
                { ManagedType: KnownManagedType.Object, IsTSExport: true } => "GetPropertyAsJSObjectTask",
                { ManagedType: KnownManagedType.Object, IsTSExport: false } => "GetPropertyAsObjectTask",
                { ManagedType: KnownManagedType.Nullable } returnTypeInfo => returnTypeInfo.TypeArgument switch
                {
                    { ManagedType: KnownManagedType.JSObject } => "GetPropertyAsJSObjectTask",
                    { ManagedType: KnownManagedType.Object, IsTSExport: true } => "GetPropertyAsJSObjectTask", // exported object types have a FromJSObject mapper
                    { ManagedType: KnownManagedType.Object, IsTSExport: false } => "GetPropertyAsObjectTask", // exported object types have a FromJSObject mapper
                    _ => throw new InvalidOperationException($"Task of nullable type '{returnTypeInfo?.ManagedType}' cannot be marshalled through TypeShim JSObject extensions"),
                },
                _ => throw new InvalidOperationException($"Task of type '{typeInfo.TypeArgument?.ManagedType}' cannot be marshalled through TypeShim JSObject extensions"),
            },
            _ => throw new InvalidOperationException($"Type '{typeInfo.ManagedType}' cannot be marshalled through JSObject nor TypeShim JSObject extensions"),
        };
    }
}