using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using TypeShim.Generator;
using TypeShim.Generator.Parsing;

internal sealed class MethodInfoBuilder(INamedTypeSymbol classSymbol, IMethodSymbol memberMethod)
{
    private readonly MethodParameterInfoBuilder parameterInfoBuilder = new(classSymbol, memberMethod);
    private readonly InteropTypeInfoBuilder typeInfoBuilder = new(memberMethod.ReturnType);

    internal MethodInfo Build()
    {
        if (memberMethod.DeclaredAccessibility != Accessibility.Public ||
            memberMethod.MethodKind is not MethodKind.Ordinary and not MethodKind.PropertyGet and not MethodKind.PropertySet)
        {
            throw new UnsupportedMethodException($"Method {classSymbol}.{memberMethod} must be of kind 'Ordinary', 'PropertyGet' or 'PropertySet' and have accessibility 'Public'.");
        }

        IReadOnlyCollection<MethodParameterInfo> parameters = [.. parameterInfoBuilder.Build()];
        InteropTypeInfo returnType = typeInfoBuilder.Build();
        MethodInfo baseMethod = new()
        {
            IsStatic = memberMethod.IsStatic,
            Name = memberMethod.Name,
            MethodParameters = parameters,
            ReturnType = returnType,
            SnapshotOverloads = [.. GenerateOverloadsWithJSObjectParameters(parameters, memberMethod.Name, memberMethod.IsStatic, returnType)]
        };

        return baseMethod;
    }

    private IEnumerable<MethodInfo> GenerateOverloadsWithJSObjectParameters(IReadOnlyCollection<MethodParameterInfo> parameters, string name, bool isStatic, InteropTypeInfo returnType)
    {
        foreach (JSObjectOverload overload in WithJSObjectParameters(parameters))
        {
            string subsetSuffix = string.Join("", overload.SwappedParameterIndices);
            yield return new MethodInfo
            {
                IsStatic = isStatic,
                Name = $"{name}_{subsetSuffix}", // Method_13 for parameters at index 1 and 3 swapped for jsobject
                MethodParameters = [.. overload.ParameterInfos],
                ReturnType = returnType,
                SnapshotOverloads = []
            };
        }
    }

    private record JSObjectOverload(IEnumerable<MethodParameterInfo> ParameterInfos, int[] SwappedParameterIndices);

    private static IEnumerable<JSObjectOverload> WithJSObjectParameters(IEnumerable<MethodParameterInfo> methodParameters)
    {
        int[] indicesToSwap = [.. methodParameters
            .Select((param, index) => (param, index))
            .Where(t => t.param.Type.RequiresCLRTypeConversion && !t.param.IsInjectedInstanceParameter)
            .Select(t => t.index)];

        if (indicesToSwap.Length == 0)
        {
            yield break;
        }

        foreach (int[] subset in GenerateSubsets(indicesToSwap))
        {
            if (subset.Length == 0) continue;

            MethodParameterInfo[] newParameters = [.. methodParameters];
            foreach (var parameterIndex in subset)
            {
                newParameters[parameterIndex] = newParameters[parameterIndex].WithJSObjectTypeInfo();
            }

            yield return new JSObjectOverload(newParameters, subset);
        }
    }

    private static IEnumerable<int[]> GenerateSubsets(int[] items)
    {
        int n = items.Length;
        int total = 1 << n;
        for (int mask = 1; mask < total; mask++) // skip empty set (mask=0)
        {
            var subset = new List<int>(n);
            for (int i = 0; i < n; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    subset.Add(items[i]);
                }
            }
            yield return subset.ToArray();
        }
    }
}
