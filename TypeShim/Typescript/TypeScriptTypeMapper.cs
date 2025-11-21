using TypeShim.Parsing;

namespace DotnetWasmTypescript.InteropGenerator.Typescript;

internal class TypeScriptTypeMapper(IEnumerable<ClassInfo> classInfos)
{
    private readonly HashSet<string> _customTypeNames = [.. classInfos.Select(ci => ci.Name)];

    public bool HasKnownType(string nameHint)
    {
        return _customTypeNames.Contains(nameHint);
    }

    public string ToTypeScriptType(KnownManagedType type, string nameHint)
    {
        if (_customTypeNames.Contains(nameHint)) return nameHint;

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
            KnownManagedType.JSObject => "object",
            KnownManagedType.Object => "object",
            KnownManagedType.String => "string",
            KnownManagedType.Exception => "Error",
            KnownManagedType.DateTime => "Date",
            KnownManagedType.DateTimeOffset => "Date",
            KnownManagedType.Nullable => "number | null", // generic fallback, could be more precise
            KnownManagedType.Task => "Promise<any>",  // could be mapped more precisely
            KnownManagedType.Array => "any[]",
            KnownManagedType.ArraySegment => "any[]",
            KnownManagedType.Span => "any[]",
            KnownManagedType.Action => "(() => void)",
            KnownManagedType.Function => "Function",
            KnownManagedType.Unknown => "any",
            _ => "any"
        };
    }
}