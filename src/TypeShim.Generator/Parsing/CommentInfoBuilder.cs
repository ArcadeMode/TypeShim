using Microsoft.CodeAnalysis;
using System.Text;
using System.Xml.Linq;

namespace TypeShim.Generator.Parsing;

internal sealed class CommentInfoBuilder(ISymbol symbol)
{
    internal CommentInfo? Build()
    {
        string? xmlCommentString = symbol.GetDocumentationCommentXml();
        
        if (string.IsNullOrWhiteSpace(xmlCommentString))
        {
            return null;
        }

        try
        {
            XDocument xmlDoc = XDocument.Parse(xmlCommentString);
            XElement? root = xmlDoc.Root;

            if (root == null)
            {
                return null;
            }

            string description = BuildDescription(root);
            IReadOnlyCollection<ParameterCommentInfo> parameters = BuildParameters(root);
            string? returns = BuildReturns(root);
            IReadOnlyCollection<ThrowsCommentInfo> throws = BuildThrows(root);

            // Only return CommentInfo if there's any actual content
            if (string.IsNullOrWhiteSpace(description) && 
                parameters.Count == 0 && 
                string.IsNullOrWhiteSpace(returns) && 
                throws.Count == 0)
            {
                return null;
            }

            return new CommentInfo
            {
                Description = description,
                Parameters = parameters,
                Returns = returns,
                Throws = throws
            };
        }
        catch
        {
            // If XML parsing fails, return null
            return null;
        }
    }

    private string BuildDescription(XElement root)
    {
        StringBuilder description = new();

        // Get summary tag
        XElement? summary = root.Element("summary");
        if (summary != null)
        {
            string summaryText = ProcessInnerTags(summary);
            description.Append(summaryText.Trim());
        }

        // Get remarks tag
        XElement? remarks = root.Element("remarks");
        if (remarks != null)
        {
            string remarksText = ProcessInnerTags(remarks);
            if (description.Length > 0 && !string.IsNullOrWhiteSpace(remarksText))
            {
                description.Append("\n\n");
            }
            description.Append(remarksText.Trim());
        }

        return description.ToString();
    }

    private IReadOnlyCollection<ParameterCommentInfo> BuildParameters(XElement root)
    {
        List<ParameterCommentInfo> parameters = [];

        foreach (XElement param in root.Elements("param"))
        {
            string? name = param.Attribute("name")?.Value;
            if (!string.IsNullOrWhiteSpace(name))
            {
                string description = ProcessInnerTags(param).Trim();
                parameters.Add(new ParameterCommentInfo
                {
                    Name = name,
                    Description = description
                });
            }
        }

        return parameters;
    }

    private string? BuildReturns(XElement root)
    {
        XElement? returns = root.Element("returns");
        if (returns == null)
        {
            return null;
        }

        string returnsText = ProcessInnerTags(returns).Trim();
        return string.IsNullOrWhiteSpace(returnsText) ? null : returnsText;
    }

    private IReadOnlyCollection<ThrowsCommentInfo> BuildThrows(XElement root)
    {
        List<ThrowsCommentInfo> throws = [];

        foreach (XElement exception in root.Elements("exception"))
        {
            string? cref = exception.Attribute("cref")?.Value;
            if (!string.IsNullOrWhiteSpace(cref))
            {
                // Remove "T:" prefix from cref if present
                string type = cref.StartsWith("T:") ? cref.Substring(2) : cref;
                string description = ProcessInnerTags(exception).Trim();
                throws.Add(new ThrowsCommentInfo
                {
                    Type = type,
                    Description = description
                });
            }
        }

        return throws;
    }

    private string ProcessInnerTags(XElement element)
    {
        StringBuilder result = new();

        foreach (XNode node in element.Nodes())
        {
            if (node is XText textNode)
            {
                result.Append(textNode.Value);
            }
            else if (node is XElement innerElement)
            {
                result.Append(TransformInnerTag(innerElement));
            }
        }

        // Normalize whitespace: replace multiple whitespaces/newlines with single space
        string text = result.ToString();
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
        return text;
    }

    private string TransformInnerTag(XElement element)
    {
        string tagName = element.Name.LocalName.ToLowerInvariant();
        string innerText = ProcessInnerTags(element);

        return tagName switch
        {
            // Code tags
            "code" => $"`{innerText}`",
            "c" => $"`{innerText}`",
            
            // Bold tags (not standard in JSDoc, but we can use markdown-style)
            "b" => $"**{innerText}**",
            "strong" => $"**{innerText}**",
            
            // Italic tags
            "i" => $"*{innerText}*",
            "em" => $"*{innerText}*",
            
            // References - just use the reference name
            "see" => GetReferenceText(element),
            "seealso" => GetReferenceText(element),
            
            // Parameter references
            "paramref" => $"`{element.Attribute("name")?.Value ?? ""}`",
            "typeparamref" => $"`{element.Attribute("name")?.Value ?? ""}`",
            
            // Para (paragraph) - just return the text
            "para" => innerText,
            
            // Lists - simplified representation
            "list" => ProcessList(element),
            
            // Default: just return inner text
            _ => innerText
        };
    }

    private string GetReferenceText(XElement element)
    {
        string? cref = element.Attribute("cref")?.Value;
        if (!string.IsNullOrWhiteSpace(cref))
        {
            // Remove prefix like "T:", "M:", "P:" etc.
            int colonIndex = cref.IndexOf(':');
            if (colonIndex >= 0 && colonIndex < cref.Length - 1)
            {
                cref = cref[(colonIndex + 1)..];
            }
            
            // Get just the type/member name without namespace
            int lastDotIndex = cref.LastIndexOf('.');
            if (lastDotIndex >= 0 && lastDotIndex < cref.Length - 1)
            {
                cref = cref[(lastDotIndex + 1)..];
            }
            
            // Remove parameter list if present (e.g., "Method(System.String)" -> "Method")
            int parenIndex = cref.IndexOf('(');
            if (parenIndex >= 0)
            {
                cref = cref[..parenIndex];
            }
            
            return $"`{cref}`";
        }
        
        // If no cref, get inner text
        string innerText = ProcessInnerTags(element);
        return string.IsNullOrWhiteSpace(innerText) ? "" : $"`{innerText}`";
    }

    private string ProcessList(XElement listElement)
    {
        // Simplified list processing - just extract items
        StringBuilder result = new();
        foreach (XElement item in listElement.Descendants("item"))
        {
            XElement? term = item.Element("term");
            XElement? description = item.Element("description");
            
            if (term != null || description != null)
            {
                result.Append("- ");
                if (term != null)
                {
                    result.Append(ProcessInnerTags(term).Trim());
                    if (description != null)
                    {
                        result.Append(": ");
                    }
                }
                if (description != null)
                {
                    result.Append(ProcessInnerTags(description).Trim());
                }
                result.Append(" ");
            }
        }
        return result.ToString();
    }
}
