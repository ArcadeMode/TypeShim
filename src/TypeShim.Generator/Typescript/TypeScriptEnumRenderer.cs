using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptEnumRenderer(RenderContext ctx)
{
    internal void Render()
    {
        EnumInfo enumInfo = ctx.NamedType as EnumInfo
            ?? throw new InvalidOperationException("TypeScriptEnumRenderer requires an enum in the RenderContext");

        TypeScriptJSDocRenderer.RenderJSDoc(ctx, enumInfo.Comment);
        ctx.Append("export enum ").Append(enumInfo.Name).AppendLine(" {");
        using (ctx.Indent())
        {
            foreach (EnumMemberInfo member in enumInfo.Members)
            {
                TypeScriptJSDocRenderer.RenderJSDoc(ctx, member.Comment);
                ctx.Append(member.Name).Append(" = ").Append(member.Value.ToString()).AppendLine(",");
            }
        }
        ctx.AppendLine("}");
    }
}
