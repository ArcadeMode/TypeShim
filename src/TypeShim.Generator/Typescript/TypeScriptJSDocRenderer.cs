using System.Text;
using System.Xml.Linq;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class TypeScriptJSDocRenderer
{
    internal static void RenderJSDoc(RenderContext ctx, CommentInfo? comment)
    {
        if (comment == null)
        {
            return;
        }

        ctx.AppendLine("/**");

        if (!string.IsNullOrWhiteSpace(comment.Description))
        {
            RenderCommentLines(ctx, comment.Description);
        }

        if (!string.IsNullOrWhiteSpace(comment.Remarks))
        {
            ctx.AppendLine(" * @remarks");
            RenderCommentLines(ctx, comment.Remarks);
        }

        foreach (ParameterCommentInfo param in comment.Parameters)
        {
            string paramDescription = param.Description.Replace(Environment.NewLine, " ").Trim();
            ctx.Append(" * @param ").Append(param.Name).Append(" - ").AppendLine(paramDescription);
        }

        if (!string.IsNullOrWhiteSpace(comment.Returns))
        {
            string returns = comment.Returns.Replace(Environment.NewLine, " ").Trim();
            ctx.Append(" * @returns ").AppendLine(returns);
        }

        foreach (ThrowsCommentInfo throwsInfo in comment.Throws)
        {
            string description = throwsInfo.Description.Replace(Environment.NewLine, " ").Trim();
            ctx.Append(" * @throws {").Append(throwsInfo.Type).Append("} ").AppendLine(description);
        }

        ctx.AppendLine(" */");
    }

    private static void RenderCommentLines(RenderContext ctx, string text)
    {
        foreach (string line in text.Split(Environment.NewLine))
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine))
            {
                ctx.AppendLine(" *");
            }
            else
            {
                ctx.Append(" * ").AppendLine(trimmedLine);
            }
        }
    }
}
