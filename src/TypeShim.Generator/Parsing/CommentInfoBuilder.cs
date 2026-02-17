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

        xmlCommentString = xmlCommentString.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

        try
        {
            XDocument xmlDoc = XDocument.Parse(xmlCommentString);
            XElement? root = xmlDoc.Root;

            if (root == null)
            {
                return null;
            }

            CommentInfo commentInfo = new()
            {
                Description = BuildDescription(root),
                Parameters = BuildParameters(root),
                Returns = BuildReturns(root),
                Throws = BuildThrows(root)
            };

            return commentInfo.IsEmpty() ? null : commentInfo;
        }
        catch
        {
            return null;
        }
    }

    private string BuildDescription(XElement root)
    {
        StringBuilder description = new();

        XElement? summary = root.Element("summary");
        if (summary != null)
        {
            description.Append(ProcessFormatTags(summary).Trim());
        }

        XElement? remarks = root.Element("remarks");
        if (remarks == null)
        {
            return description.ToString();
        }

        string remarksText = ProcessFormatTags(remarks);
        if (description.Length > 0 && !string.IsNullOrWhiteSpace(remarksText))
        {
            description.Append(Environment.NewLine);
            description.Append(Environment.NewLine);
        }
        description.Append(remarksText.Trim());

        return description.ToString();
    }

    private IReadOnlyCollection<ParameterCommentInfo> BuildParameters(XElement root)
    {
        List<ParameterCommentInfo> parameters = [];

        foreach (XElement param in root.Elements("param"))
        {
            string? name = param.Attribute("name")?.Value;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            string description = ProcessFormatTags(param).Trim();
            parameters.Add(new ParameterCommentInfo
            {
                Name = name,
                Description = description
            });
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

        string returnsText = ProcessFormatTags(returns).Trim();
        return string.IsNullOrWhiteSpace(returnsText) ? null : returnsText;
    }

    private IReadOnlyCollection<ThrowsCommentInfo> BuildThrows(XElement root)
    {
        List<ThrowsCommentInfo> throws = [];

        foreach (XElement exception in root.Elements("exception"))
        {
            string? cref = exception.Attribute("cref")?.Value;
            if (string.IsNullOrWhiteSpace(cref))
            {
                continue;
            }

            string type = ExtractTypeFromCref(cref);
            if (string.IsNullOrWhiteSpace(type))
            {
                continue;
            }

            string description = ProcessFormatTags(exception).Trim();
            throws.Add(new ThrowsCommentInfo
            {
                Type = type,
                Description = description
            });
        }

        return throws;
    }

    private static string ExtractTypeFromCref(string cref)
    {
        string[] parts = cref.Split(':');
        string type = parts[^1];
        
        if (type.StartsWith("!"))
        {
            type = type[1..];
        }
        
        return type.Trim();
    }

    private string ProcessFormatTags(XElement element)
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

        string text = result.ToString();
        
        // Normalize whitespace (multiple spaces/tabs become single space)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[ \t]+", " ");
        
        return text;
    }

    private string TransformInnerTag(XElement element)
    {
        string tagName = element.Name.LocalName.ToLowerInvariant();
        string innerText = ProcessFormatTags(element);

        return tagName switch
        {
            "code" or "c" or "example" => $"`{innerText}`",
            "b" or "strong" => $"**{innerText}**",
            "i" or "em" => $"*{innerText}*",
            "br" => Environment.NewLine,
            "see" or "seealso" => GetReferenceText(element),
            "paramref" or "typeparamref" => $"`{element.Attribute("name")?.Value ?? ""}`",
            "para" => innerText,
            "list" => ProcessList(element),
            _ => innerText
        };
    }

    private string GetReferenceText(XElement element)
    {
        string? cref = element.Attribute("cref")?.Value;
        if (!string.IsNullOrWhiteSpace(cref))
        {
            string type = ExtractTypeFromCref(cref);
            
            // Get just the type/member name without namespace
            int lastDotIndex = type.LastIndexOf('.');
            if (lastDotIndex >= 0 && lastDotIndex < type.Length - 1)
            {
                type = type[(lastDotIndex + 1)..];
            }
            
            // Remove parameter list if present (e.g., "Method(System.String)" -> "Method")
            int parenIndex = type.IndexOf('(');
            if (parenIndex >= 0)
            {
                type = type[..parenIndex];
            }
            
            return $"`{type}`";
        }
        
        string innerText = ProcessFormatTags(element);
        return string.IsNullOrWhiteSpace(innerText) ? "" : $"`{innerText}`";
    }

    private string ProcessList(XElement listElement)
    {
        StringBuilder result = new();
        result.Append(Environment.NewLine);
        
        XElement? listHeader = listElement.Element("listheader");
        if (listHeader != null)
        {
            RenderListItem(listHeader, result);
        }
        
        foreach (XElement item in listElement.Elements("item"))
        {
            RenderListItem(item, result);
        }
        
        return result.ToString();
    }

    private void RenderListItem(XElement item, StringBuilder result)
    {
        XElement? term = item.Element("term");
        XElement? description = item.Element("description");
        
        if (term != null || description != null)
        {
            result.Append("- ");
            if (term != null)
            {
                result.Append(ProcessFormatTags(term).Trim());
                if (description != null)
                {
                    result.Append(": ");
                }
            }
            if (description != null)
            {
                result.Append(ProcessFormatTags(description).Trim());
            }
            result.Append(Environment.NewLine);
        }
        else
        {
            string itemText = ProcessFormatTags(item).Trim();
            if (!string.IsNullOrWhiteSpace(itemText))
            {
                result.Append(itemText);
                result.Append(Environment.NewLine);
            }
        }
    }
}
