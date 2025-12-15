using Microsoft.CodeAnalysis;
using TypeShim.Generator.Parsing;

internal sealed class MethodInfo
{
    internal required bool IsStatic { get; init; }
    internal required string Name { get; init; }
    internal required IReadOnlyCollection<MethodParameterInfo> MethodParameters { get; init; }
    internal required InteropTypeInfo ReturnType { get; init; }

    public MethodInfo WithoutInstanceParameter()
    {
        return new MethodInfo
        {
            IsStatic = this.IsStatic,
            Name = this.Name,
            MethodParameters = [.. this.MethodParameters.Where(p => !p.IsInjectedInstanceParameter)],
            ReturnType = this.ReturnType,
        };
    }

    public MethodInfo WithInteropTypeInfo()
    {
        return new MethodInfo
        {
            IsStatic = this.IsStatic,
            Name = this.Name,
            MethodParameters = [.. this.MethodParameters.Select(p => p.WithInteropTypeInfo())],
            ReturnType = this.ReturnType.AsInteropTypeInfo(),
        };
    }

    public MethodInfo? WithJSObjectParameters()
    {
        int[] indicesToSwap = [.. this.MethodParameters
            .Select((param, index) => (param, index))
            .Where(t => t.param.Type.RequiresCLRTypeConversion && !t.param.IsInjectedInstanceParameter)
            .Select(t => t.index)];

        if (indicesToSwap.Length == 0)
        {
            return null;
        }

        MethodParameterInfo[] newParameters = [.. this.MethodParameters];
        foreach (var parameterIndex in indicesToSwap)
        {
            newParameters[parameterIndex] = newParameters[parameterIndex].WithJSObjectTypeInfo();
        }

        return new MethodInfo
        {
            IsStatic = this.IsStatic,
            Name = $"_{this.Name}", //TODO: Decide on a better naming strategy for JSObject methods BEEPBOOP
            MethodParameters = newParameters,
            ReturnType = this.ReturnType,
        };
    }
}
