using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using TypeShim.Shared;
using TypeShim.Generator;
using TypeShim.Generator.Parsing;

internal sealed class MethodInfoBuilder(INamedTypeSymbol classSymbol, IMethodSymbol memberMethod, InteropTypeInfoCache typeInfoCache)
{
    private readonly MethodParameterInfoBuilder parameterInfoBuilder = new(classSymbol, memberMethod, typeInfoCache);
    private readonly InteropTypeInfoBuilder typeInfoBuilder = new(memberMethod.ReturnType, typeInfoCache);

    internal MethodInfo Build()
    {
        if (memberMethod.DeclaredAccessibility != Accessibility.Public ||
            memberMethod.MethodKind is not MethodKind.Ordinary and not MethodKind.PropertyGet and not MethodKind.PropertySet and not MethodKind.Constructor)
        {
            throw new NotSupportedMethodException($"Method {classSymbol}.{memberMethod} must be of kind 'Ordinary', 'PropertyGet', 'PropertySet' or 'Constructor' and have accessibility 'Public'.");
        }

        IReadOnlyCollection<MethodParameterInfo> parameters = [.. parameterInfoBuilder.Build()];
        return new MethodInfo()
        {
            IsStatic = memberMethod.IsStatic,
            Name = memberMethod.Name,
            Parameters = parameters,
            ReturnType = typeInfoBuilder.Build(),
            Comment = new CommentInfoBuilder(memberMethod).Build(),
        };
    }
}
