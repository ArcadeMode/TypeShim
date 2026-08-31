using TypeShim.Shared;

namespace TypeShim.Generator.Typescript;

internal static class TypeScriptSymbolNameResolver
{
    internal static string ResolveSimpleInteropTypeSymbol(InteropTypeInfo typeInfo)
    {
        return typeInfo.ManagedType switch
        {
            KnownManagedType.Object // objects are represented differently on the interop boundary
                => "ManagedObject",
            KnownManagedType.Char // chars are represented as numbers on the interop boundary (is intended: https://github.com/dotnet/runtime/issues/123187)
                => "number",
            _ when typeInfo.IsEnum => "number", // enums cross the interop boundary as number
            _ => ResolveSimpleTypeSymbol(typeInfo)
        };
    }

    internal static string ResolveSimpleTypeSymbol(InteropTypeInfo typeInfo)
    {
        if (typeInfo.IsEnum)
        {
            return typeInfo.CSharpTypeSyntax.ToString();
        }
        return typeInfo.ManagedType switch
        {
            KnownManagedType.Object when typeInfo.RequiresTypeConversion && typeInfo.SupportsTypeConversion
                => typeInfo.CSharpTypeSyntax.ToString(),
            KnownManagedType.Object when typeInfo.RequiresTypeConversion && !typeInfo.SupportsTypeConversion
                => "ManagedObject",
            KnownManagedType.Object when !typeInfo.RequiresTypeConversion
                => "ManagedObject",

            KnownManagedType.None => "undefined",
            KnownManagedType.Void => "void",
            KnownManagedType.JSObject
                => "object",

            KnownManagedType.Boolean => "boolean",
            KnownManagedType.Char
            or KnownManagedType.String => "string",
            KnownManagedType.Byte
            or KnownManagedType.Int16
            or KnownManagedType.Int32
            or KnownManagedType.Int64
            or KnownManagedType.Double
            or KnownManagedType.Single
            or KnownManagedType.IntPtr
                => "number",
            KnownManagedType.DateTime
            or KnownManagedType.DateTimeOffset => "Date",
            KnownManagedType.Exception => "Error",

            KnownManagedType.Unknown
            or _ => "any",
        };
    }

    internal static string ResolveMemoryViewTypeArgSymbol(InteropTypeInfo typeInfo)
    {
        if (typeInfo.ManagedType is not KnownManagedType.Span and not KnownManagedType.ArraySegment)
        {
            throw new InvalidOperationException($"Type '{typeInfo.ManagedType}' is not a valid MemoryView type.");
        }

        return typeInfo.TypeArgument switch
        {
            { ManagedType: KnownManagedType.Byte } => "Uint8Array",
            { ManagedType: KnownManagedType.Int32 } => "Int32Array",
            { ManagedType: KnownManagedType.Double } => "Float64Array",
            _ => throw new InvalidOperationException($"Type argument '{typeInfo.TypeArgument?.ManagedType}' is not valid for MemoryView types.")
        };
    }
}