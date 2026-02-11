namespace TypeShim.Generator;

internal sealed class RenderOptions
{
    internal required int IndentSpaces { get; init; }

    internal static RenderOptions CSharp = new()
    {
        IndentSpaces = 4
    };

    internal static RenderOptions TypeScript = new()
    {
        IndentSpaces = 2
    };
}
