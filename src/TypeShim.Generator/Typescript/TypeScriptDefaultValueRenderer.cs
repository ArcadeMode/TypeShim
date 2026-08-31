using System.Globalization;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptDefaultValueRenderer(RenderContext ctx)
{
    /// <summary>
    /// Renders <paramref name="def"/> as a TypeScript literal
    /// </summary>
    /// <exception cref="NotSupportedDefaultValueException">when the default cannot be rendered as valid TypeScript</exception>
    internal void Render(InteropTypeInfo type, ParameterDefaultInfo def)
    {
        if (def.Value is null)
        {
            if (type.IsNullableType)
            {
                ctx.Append("null");
                return;
            }
            
            if (type.ManagedType is KnownManagedType.DateTime or KnownManagedType.DateTimeOffset)
            {
                // Use .NET DateTime/DateTimeOffset default (0001-01-01) in TypeScript (no inconsistency with JS Date default starting in the year -271,821)
                ctx.Append("new Date(\"0001-01-01T00:00:00Z\")");
                return;
            }

            // 'x: T = null' would violate TypeScript strictNullChecks, user can fix themselves by making their type nullable.
            throw new NotSupportedDefaultValueException(
                $"Null default values for reference type '{type.CSharpTypeSyntax}' are not yet supported.");
        }

        InteropTypeInfo valueType = type.IsNullableType && type.TypeArgument is not null ? type.TypeArgument : type;

        if (valueType.IsEnum && ctx.SymbolMap.GetNamedTypeInfo(valueType) is EnumInfo enumInfo
            && enumInfo.GetMemberByValue(Convert.ToInt64(def.Value, CultureInfo.InvariantCulture)) is string memberName)
        {
            ctx.Append(enumInfo.Name).Append('.').Append(memberName);
            return;
        }

        string literal = valueType.ManagedType switch
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

        ctx.Append(literal);
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
