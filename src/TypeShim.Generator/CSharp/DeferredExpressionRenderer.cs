namespace TypeShim.Generator.CSharp;

internal class DeferredExpressionRenderer(Action renderAction)
{
    internal required bool IsBinary { get; init; }
    internal void Render() => renderAction();
    
    public static DeferredExpressionRenderer FromBinary(Action renderAction) => new(renderAction){ IsBinary = true };
    public static DeferredExpressionRenderer FromUnary(Action renderAction) => new(renderAction){ IsBinary = false };
}
