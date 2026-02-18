using Microsoft.CodeAnalysis;
using System.Text;
using System.Xml.Linq;

namespace TypeShim.Generator.Parsing;

internal sealed partial class CommentInfoBuilder(ISymbol symbol)
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
            XDocument xmlDoc = XDocument.Parse(xmlCommentString.Replace("\n", " ").Replace("\r", " "));
            XElement? root = xmlDoc.Root;
            if (root == null)
            {
                return null;
            }
            CommentInfo commentInfo = new()
            {
                Description = BuildFormattedTextElement(root, "summary"),
                Remarks = BuildFormattedTextElement(root, "remarks"),
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

    private static string BuildFormattedTextElement(XElement root, string elementName)
    {
        if (root.Element(elementName) is XElement element)
        {
            return ProcessFormatXMLElement(element).Trim();
        }
        return string.Empty;
    }

    private static List<ParameterCommentInfo> BuildParameters(XElement root)
    {
        List<ParameterCommentInfo> parameters = [];
        foreach (XElement param in root.Elements("param"))
        {
            string? name = param.Attribute("name")?.Value;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            string description = ProcessFormatXMLElement(param).Trim();
            parameters.Add(new ParameterCommentInfo
            {
                Name = name,
                Description = description
            });
        }
        return parameters;
    }

    private static string? BuildReturns(XElement root)
    {
        XElement? returns = root.Element("returns");
        if (returns == null)
        {
            return null;
        }

        string returnsText = ProcessFormatXMLElement(returns).Trim();
        return string.IsNullOrWhiteSpace(returnsText) ? null : returnsText;
    }

    private static IReadOnlyCollection<ThrowsCommentInfo> BuildThrows(XElement root)
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

            string description = ProcessFormatXMLElement(exception).Trim();
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
        if (type.StartsWith('!'))
        {
            type = type[1..];
        }
        return type.Trim();
    }

    private static string ProcessXMLElement(XElement element)
    {
        string tagName = element.Name.LocalName.ToLowerInvariant();
        string innerText = ProcessFormatXMLElement(element);
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
            "a" => ProcessAnchor(element),
            _ => innerText
        };
    }

    private static string ProcessFormatXMLElement(XElement element)
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
                result.Append(ProcessXMLElement(innerElement));
            }
        }
        return GetMultiWhitespaceRegex().Replace(result.ToString(), " "); // Normalize whitespace
    }

    private static string ProcessAnchor(XElement element)
    {
        string? href = element.Attribute("href")?.Value;
        if (string.IsNullOrEmpty(href))
        {
            return element.Value;
        }
        return $"{{@link {href} | {ProcessFormatXMLElement(element)}}}";
    }

    private static string GetReferenceText(XElement element)
    {
        string? cref = element.Attribute("cref")?.Value;
        if (!string.IsNullOrWhiteSpace(cref))
        {
            string type = ExtractTypeFromCref(cref);
            // Remove parameter list if present (e.g., "Method(System.String)" -> "Method")
            int parenIndex = type.IndexOf('(');
            if (parenIndex >= 0)
            {
                type = type[..parenIndex].Trim();
            }            
            return $"`{type}`";
        }
        
        string innerText = ProcessFormatXMLElement(element);
        return string.IsNullOrWhiteSpace(innerText) ? string.Empty : $"`{innerText}`";
    }

    private static string ProcessList(XElement listElement)
    {
        StringBuilder result = new();
        result.Append(Environment.NewLine);
        
        XElement? listHeader = listElement.Element("listheader");
        if (listHeader != null)
        {
            ProcessListItem(listHeader, result);
        }
        
        foreach (XElement item in listElement.Elements("item"))
        {
            ProcessListItem(item, result);
        }
        
        return result.ToString();

        static void ProcessListItem(XElement item, StringBuilder result)
        {
            XElement? term = item.Element("term");
            XElement? description = item.Element("description");

            if (term != null || description != null)
            {
                result.Append("- ");
                if (term != null)
                {
                    result.Append(ProcessFormatXMLElement(term).Trim());
                    if (description != null)
                    {
                        result.Append(": ");
                    }
                }
                if (description != null)
                {
                    result.Append(ProcessFormatXMLElement(description).Trim());
                }
                result.Append(Environment.NewLine);
            }
            else
            {
                string itemText = ProcessFormatXMLElement(item).Trim();
                if (!string.IsNullOrWhiteSpace(itemText))
                {
                    result.Append(itemText);
                    result.Append(Environment.NewLine);
                }
            }
        }
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"[ \t]+")]
    private static partial System.Text.RegularExpressions.Regex GetMultiWhitespaceRegex();
}
