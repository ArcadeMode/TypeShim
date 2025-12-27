using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using TypeShim.Shared;
using TypeShim.Generator;
using TypeShim.Generator.Parsing;

internal sealed class MethodInfoBuilder(INamedTypeSymbol classSymbol, IMethodSymbol memberMethod, InteropTypeInfoCache typeInfoCache)
{
    private readonly MethodParameterInfoBuilder parameterInfoBuilder = new(classSymbol, memberMethod, typeInfoCache);

    internal MethodInfo Build()
    {
        if (memberMethod.DeclaredAccessibility != Accessibility.Public ||
            memberMethod.MethodKind is not MethodKind.Ordinary and not MethodKind.PropertyGet and not MethodKind.PropertySet and not MethodKind.Constructor)
        {
            throw new UnsupportedMethodException($"Method {classSymbol}.{memberMethod} must be of kind 'Ordinary', 'PropertyGet', 'PropertySet' or 'Constructor' and have accessibility 'Public'.");
        }

        IReadOnlyCollection<MethodParameterInfo> parameters = [.. parameterInfoBuilder.Build()];
        bool isConstructor = memberMethod.MethodKind == MethodKind.Constructor;
        ITypeSymbol returnType = isConstructor ? classSymbol : memberMethod.ReturnType;
        InteropTypeInfoBuilder returnTypeInfoBuilder = new(returnType, typeInfoCache);
        MethodInfo baseMethod = new()
        {
            IsStatic = memberMethod.IsStatic,
            IsConstructor = isConstructor,
            Name = memberMethod.Name,
            MethodParameters = parameters,
            ReturnType = returnTypeInfoBuilder.Build(),
        };

        return baseMethod;
    }
}