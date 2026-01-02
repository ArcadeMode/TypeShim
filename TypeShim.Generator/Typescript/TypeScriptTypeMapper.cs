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
        return _userTypeNames.Contains(typeInfo.CSharpTypeSyntax.ToString());
    }

    public TypeScriptSymbolNameTemplate ToTypeScriptType(InteropTypeInfo typeInfo)
    {
        //if (typeInfo.RequiresTypeConversion && typeInfo.SupportsTypeConversion) 
        //    return TypeScriptSymbolNameTemplate.ForUserType(typeInfo);

        //if (typeInfo.RequiresTypeConversion && !typeInfo.SupportsTypeConversion && !typeInfo.IsTSExport && ) 
        //    return TypeScriptSymbolNameTemplate.ForSimpleType("any");

        return typeInfo.ManagedType switch
        {
            KnownManagedType.Task 
                => TypeScriptSymbolNameTemplate.ForPromiseType(typeInfo.TypeArgument != null ? ToTypeScriptType(typeInfo.TypeArgument) : null), // note: Task can be without type argument
            KnownManagedType.Array 
                => TypeScriptSymbolNameTemplate.ForArrayType(ToTypeScriptType(typeInfo.TypeArgument ?? throw new ArgumentException("Array type must have a type argument"))), 
            KnownManagedType.Nullable 
                => TypeScriptSymbolNameTemplate.ForNullableType(ToTypeScriptType(typeInfo.TypeArgument ?? throw new ArgumentException("Nullable type must have a type argument"))),
            KnownManagedType.None => TypeScriptSymbolNameTemplate.ForSimpleType("undefined"),
            KnownManagedType.Void => TypeScriptSymbolNameTemplate.ForSimpleType("void"),
            KnownManagedType.JSObject 
                => TypeScriptSymbolNameTemplate.ForSimpleType("object"),
            KnownManagedType.Object when typeInfo is { RequiresTypeConversion: true, SupportsTypeConversion: true }
                => TypeScriptSymbolNameTemplate.ForUserType(typeInfo.CSharpTypeSyntax.ToString()),
            KnownManagedType.Object when typeInfo is { RequiresTypeConversion: true, SupportsTypeConversion: false }
                => TypeScriptSymbolNameTemplate.ForSimpleType("ManagedObject"),
            KnownManagedType.Object when typeInfo is { RequiresTypeConversion: false }
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
            KnownManagedType.DateTime
            or KnownManagedType.DateTimeOffset => TypeScriptSymbolNameTemplate.ForSimpleType("Date"),
            KnownManagedType.Exception => TypeScriptSymbolNameTemplate.ForSimpleType("Error"),

            // TODO: add support for ArraySegment<T> and Span<T> i.e. MemoryView
            KnownManagedType.ArraySegment
            or KnownManagedType.Span
                => throw new NotImplementedException("ArraySegment and Span are not yet supported"),
            // TODO: add support for Action and Function types
            KnownManagedType.Action => throw new NotImplementedException("Action is not yet supported"), // "(() => void)"
            KnownManagedType.Function => throw new NotImplementedException("Function is not yet supported"), // "Function"
            
            KnownManagedType.Unknown 
            or _ => TypeScriptSymbolNameTemplate.ForSimpleType("any"),
        };
    }
}