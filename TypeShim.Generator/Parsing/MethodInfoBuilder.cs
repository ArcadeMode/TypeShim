using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using TypeShim.Core;
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
        };

        return baseMethod;
    }
}
