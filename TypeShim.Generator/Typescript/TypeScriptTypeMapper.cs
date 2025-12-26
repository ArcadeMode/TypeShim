using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeShim.Shared;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptTypeMapper(IEnumerable<ClassInfo> classInfos)
{
    private readonly HashSet<string> _userTypeNames = [.. classInfos.Select(ci => ci.Name)];

    public bool IsUserType(InteropTypeInfo typeInfo)
    {
        return _userTypeNames.Contains(typeInfo.CLRTypeSyntax.ToString());
    }

    public TypeScriptSymbolNameTemplate ToTypeScriptType(InteropTypeInfo typeInfo)
    {
        if (IsUserType(typeInfo)) 
            return TypeScriptSymbolNameTemplate.ForUserType(typeInfo);

        if (typeInfo.IsNullableType) 
            return TypeScriptSymbolNameTemplate.ForNullableType(ToTypeScriptType(typeInfo.TypeArgument ?? throw new ArgumentException("Nullable type must have a type argument")));

        return typeInfo.ManagedType switch
        {
            KnownManagedType.None => TypeScriptSymbolNameTemplate.ForSimpleType("undefined"),
            KnownManagedType.Void => TypeScriptSymbolNameTemplate.ForSimpleType("void"),
            KnownManagedType.JSObject 
            or KnownManagedType.Object
                => TypeScriptSymbolNameTemplate.ForSimpleType("object"),
            KnownManagedType.Boolean => TypeScriptSymbolNameTemplate.ForSimpleType("boolean"),
            KnownManagedType.Char 
            or KnownManagedType.String => TypeScriptSymbolNameTemplate.ForSimpleType("string"),
            KnownManagedType.Byte 
            or KnownManagedType.Int16 
            or KnownManagedType.Int32 
            or KnownManagedType.Int64 
            or KnownManagedType.Double 
            or KnownManagedType.Single 
            or KnownManagedType.IntPtr 
                => TypeScriptSymbolNameTemplate.ForSimpleType("number"),
            KnownManagedType.Exception => TypeScriptSymbolNameTemplate.ForSimpleType("Error"),
            KnownManagedType.DateTime
            or KnownManagedType.DateTimeOffset => TypeScriptSymbolNameTemplate.ForSimpleType("Date"),
            KnownManagedType.Task => TypeScriptSymbolNameTemplate.ForPromiseType(ToTypeScriptType(typeInfo.TypeArgument ?? typeInfo)),
            KnownManagedType.Array 
                => TypeScriptSymbolNameTemplate.ForArrayType(ToTypeScriptType(typeInfo.TypeArgument ?? typeInfo)),
            KnownManagedType.ArraySegment
            or KnownManagedType.Span
                => throw new NotImplementedException("ArraySegment and Span are not yet supported"), // MemoryView TODO
            KnownManagedType.Nullable => throw new NotImplementedException("Nullable value-types are not yet supported"), // return "something | null" ?
            KnownManagedType.Action => throw new NotImplementedException("Action is not yet supported"), // "(() => void)"
            KnownManagedType.Function => throw new NotImplementedException("Function is not yet supported"), // "Function"
            KnownManagedType.Unknown 
            or _ => TypeScriptSymbolNameTemplate.ForSimpleType("any"),
        };
    }
}