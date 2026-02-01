namespace TypeShim.Generator.CSharp;

internal class DeferredExpressionRenderer(Action renderAction)
{
    internal void Render() => renderAction();

    public static implicit operator DeferredExpressionRenderer(Action renderAction)
    {
        return new DeferredExpressionRenderer(renderAction);
    }
    public static DeferredExpressionRenderer From(Action renderAction) => new(renderAction);

}