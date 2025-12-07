using Microsoft.CodeAnalysis;
using TypeShim.Generator;

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

        return new MethodInfo
        {
            IsStatic = memberMethod.IsStatic,
            Name = memberMethod.Name,
            MethodParameters = [.. parameterInfoBuilder.Build()],
            ReturnType = typeInfoBuilder.Build()
            //TODO: exception nicer
            ?? throw new InvalidOperationException($"Could not create InteropTypeInfo for return type {memberMethod.ReturnType} of method {memberMethod} of {classSymbol}")
        };
    }
}
