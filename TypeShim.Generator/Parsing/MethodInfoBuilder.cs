using Microsoft.CodeAnalysis;

internal sealed class MethodInfoBuilder(INamedTypeSymbol classSymbol, IMethodSymbol memberMethod)
{
    private readonly MethodParameterInfoBuilder parameterInfoBuilder = new(classSymbol, memberMethod);
    private readonly InteropTypeInfoBuilder typeInfoBuilder = new(memberMethod.ReturnType);

    internal MethodInfo Build()
    {
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
