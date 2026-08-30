using System.Globalization;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Typescript;

/// <summary>
/// Formats an optional parameter's default value as a TypeScript literal for the public proxy signature.
/// </summary>
internal static class TypeScriptDefaultValueFormatter
{
    /// <summary>
    /// Formats <paramref name="def"/> as a TypeScript literal.
    /// Throws <see cref="NotSupportedDefaultValueException"/> when the default cannot be rendered as valid
    /// TypeScript, so codegen halts loudly rather than silently emitting a parameter without its default.
    /// </summary>
    internal static string Format(InteropTypeInfo type, ParameterDefaultInfo def)
    {
        if (def.Value is null)
        {
            // Only nullable value types (rendered as 'T | null') can safely take a 'null' default.
            if (type.IsNullableType)
            {
                return "null";
            }

            if (type.ManagedType is KnownManagedType.DateTime or KnownManagedType.DateTimeOffset)
            {
                throw new NotSupportedDefaultValueException(
                    $"Default value 'default' for '{type.CSharpTypeSyntax}' is not yet supported (pending Date literal support).");
            }

            // Non-nullable reference types would produce 'x: T = null', which violates strictNullChecks.
            throw new NotSupportedDefaultValueException(
                $"Null default values for reference type '{type.CSharpTypeSyntax}' are not yet supported (pending nullable type widening).");
        }

        // Unwrap nullable value types (e.g. 'int? = 5') to format the underlying value.
        InteropTypeInfo valueType = type.IsNullableType && type.TypeArgument is not null ? type.TypeArgument : type;

        return valueType.ManagedType switch
        {
            KnownManagedType.Boolean => def.Value is true ? "true" : "false",
            KnownManagedType.String => QuoteString(def.Value.ToString() ?? string.Empty),
            KnownManagedType.Char => QuoteString(def.Value.ToString() ?? string.Empty),
            KnownManagedType.Byte
            or KnownManagedType.Int16
            or KnownManagedType.Int32
            or KnownManagedType.Int64
            or KnownManagedType.IntPtr => FormatIntegral(def.Value),
            KnownManagedType.Double
            or KnownManagedType.Single => FormatFloatingPoint(def.Value),
            _ => throw new NotSupportedDefaultValueException(
                $"Default values of type '{type.CSharpTypeSyntax}' are not supported."),
        };
    }

    private static string FormatIntegral(object value) =>
        Convert.ToString(value, CultureInfo.InvariantCulture) ?? "0";

    private static string FormatFloatingPoint(object value) => value switch
    {
        double d => d.ToString("R", CultureInfo.InvariantCulture),
        float f => f.ToString("R", CultureInfo.InvariantCulture),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "0",
    };

    private static string QuoteString(string value)
    {
        StringBuilder sb = new(value.Length + 2);
        sb.Append('"');
        foreach (char c in value)
        {
            switch (c)
            {
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }
}
