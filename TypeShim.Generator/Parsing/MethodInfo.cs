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

    public IEnumerable<(MethodInfo Original, MethodInfo Permutation)> AllJSObjectParameterPermutations()
    {
        var jsObjectParameterIndices = this.MethodParameters
            .Select((param, index) => (param, index))
            .Where(t => t.param.Type.RequiresCLRTypeConversion // user types require conversion
                && !t.param.IsInjectedInstanceParameter) // skip instance parameter, cannot invoke c# methods on JSObject instance
            .Select(t => t.index)
            .ToArray();
        int permutationCount = 1 << jsObjectParameterIndices.Length;
        for (int i = 0; i < permutationCount; i++)
        {
            MethodParameterInfo[] newParameters = [.. this.MethodParameters];
            for (int bitIndex = 0; bitIndex < jsObjectParameterIndices.Length; bitIndex++)
            {
                int parameterIndex = jsObjectParameterIndices[bitIndex];
                if ((i & (1 << bitIndex)) != 0)
                {
                    newParameters[parameterIndex] = newParameters[parameterIndex].WithJSObjectTypeInfo();
                }
            }
            yield return (Original: this, Permutation: new MethodInfo
            {
                IsStatic = this.IsStatic,
                Name = this.Name,
                MethodParameters = newParameters,
                ReturnType = this.ReturnType,
            });
        }
    }
}
