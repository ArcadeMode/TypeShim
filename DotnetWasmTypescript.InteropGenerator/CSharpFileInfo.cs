using Microsoft.CodeAnalysis;

class CSharpFileInfo
{
    public string Path { get; set; } = string.Empty;
    public SyntaxTree SyntaxTree { get; set; } = null!;
}
