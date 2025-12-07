using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class TypeScriptTypeMapper(IEnumerable<ClassInfo> classInfos)
{
    private readonly HashSet<string> _userTypeNames = [.. classInfos.Select(ci => ci.Name)];

    public bool IsUserType(TypeSyntax nameHint)
    {
        return _userTypeNames.Contains(nameHint.ToString());
    }

    public TypeSyntax ExtractInnerTypeArgument(TypeSyntax typeSyntax)
    {
       return typeSyntax switch
        {
            GenericNameSyntax generic => generic.TypeArgumentList.Arguments.SingleOrDefault()
                ?? throw new Exception("Generic types are only supported with a single type argument"),
            ArrayTypeSyntax array => array.ElementType ?? SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
            _ => typeSyntax
        };
    }

    public string ToTypeScriptType(KnownManagedType type, TypeSyntax typeSyntax)
    {
        typeSyntax = GetTypeFromNullableSyntax(typeSyntax, out bool isNullable);
        if (IsUserType(typeSyntax)) return isNullable ? $"{typeSyntax} | null" : typeSyntax.ToString();

        // TODO: handle nullable types properly (i.e. int?[] => Array<int | null>
        // really need to start writing some unit tests...

        return type switch
        {
            KnownManagedType.None => "undefined",
            KnownManagedType.Void => "void",
            KnownManagedType.Boolean => "boolean",
            KnownManagedType.Byte => "number",
            KnownManagedType.Char => "string",
            KnownManagedType.Int16 => "number",
            KnownManagedType.Int32 => "number",
            KnownManagedType.Int64 => "number",
            KnownManagedType.Double => "number",
            KnownManagedType.Single => "number",
            KnownManagedType.IntPtr => "number", // JS doesn't have pointers, typically represented as number
            KnownManagedType.JSObject or KnownManagedType.Object 
                => "object",
            KnownManagedType.String => "string",
            KnownManagedType.Exception => "Error",
            KnownManagedType.DateTime => "Date",
            KnownManagedType.DateTimeOffset => "Date",
            KnownManagedType.Nullable => throw new NotImplementedException("Nullable value-types are not yet supported"), // return "something | null" ?
            KnownManagedType.Task => $"Promise<{ToTypeScriptType(KnownManagedType.Unknown, ExtractInnerTypeArgument(typeSyntax))}>",
            KnownManagedType.Array or KnownManagedType.ArraySegment or KnownManagedType.Span 
                => $"Array<{ToTypeScriptType(KnownManagedType.Unknown, ExtractInnerTypeArgument(typeSyntax))}>",
            KnownManagedType.Action => "(() => void)",
            KnownManagedType.Function => "Function", // TODO: try map signature ?
            KnownManagedType.Unknown => "any",
            _ => "any"
        };
    }

    public TypeSyntax GetTypeFromNullableSyntax(TypeSyntax typeSyntax, out bool isNullable)
    {
        if (typeSyntax is NullableTypeSyntax nullable)
        {
            isNullable = true;
            return nullable.ElementType;
        }
        isNullable = false;
        return typeSyntax;
    }
}