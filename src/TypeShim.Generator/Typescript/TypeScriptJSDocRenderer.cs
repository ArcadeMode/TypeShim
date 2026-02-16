using System.Text;
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

        // Render description
        if (!string.IsNullOrWhiteSpace(comment.Description))
        {
            // Normalize line endings to handle cross-platform scenarios
            string normalizedDescription = comment.Description.Replace("\r\n", "\n").Replace("\r", "\n");
            string[] descriptionLines = normalizedDescription.Split('\n');
            foreach (string line in descriptionLines)
            {
                string trimmedLine = line.Trim();
                // Render empty lines as blank comment lines
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    ctx.AppendLine(" *");
                }
                else
                {
                    ctx.AppendLine($" * {trimmedLine}");
                }
            }
        }

        // Add blank line before tags if there are any tags and a description
        bool hasTags = comment.Parameters.Count > 0 || 
                       !string.IsNullOrWhiteSpace(comment.Returns) || 
                       comment.Throws.Count > 0;
        if (!string.IsNullOrWhiteSpace(comment.Description) && hasTags)
        {
            ctx.AppendLine(" *");
        }

        // Render parameters
        foreach (ParameterCommentInfo param in comment.Parameters)
        {
            string description = param.Description;
            // Replace newlines in parameter descriptions with spaces
            description = description.Replace("\n", " ").Trim();
            ctx.AppendLine($" * @param {param.Name} {description}");
        }

        // Render returns
        if (!string.IsNullOrWhiteSpace(comment.Returns))
        {
            string returns = comment.Returns.Replace("\n", " ").Trim();
            ctx.AppendLine($" * @returns {returns}");
        }

        // Render throws/exceptions
        foreach (ThrowsCommentInfo throwsInfo in comment.Throws)
        {
            string description = throwsInfo.Description;
            // Replace newlines in throws descriptions with spaces
            description = description.Replace("\n", " ").Trim();
            ctx.AppendLine($" * @throws {{{throwsInfo.Type}}} {description}");
        }

        ctx.AppendLine(" */");
    }
}
