using Microsoft.CodeAnalysis;

internal class CSharpFileInfo
{
    internal SyntaxTree SyntaxTree { get; private init; } = null!;

    internal static CSharpFileInfo Create(SyntaxTree syntaxTree)
    {
        return new CSharpFileInfo
        {
            SyntaxTree = syntaxTree
        };
    }
}
