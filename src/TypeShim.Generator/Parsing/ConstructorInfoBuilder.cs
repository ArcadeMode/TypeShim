using Microsoft.CodeAnalysis;
using TypeShim.Shared;

internal sealed class ConstructorInfoBuilder(INamedTypeSymbol classSymbol, IMethodSymbol memberMethod, InteropTypeInfoCache typeInfoCache)
{
    private readonly MethodParameterInfoBuilder parameterInfoBuilder = new(classSymbol, memberMethod, typeInfoCache);
    private readonly InteropTypeInfoBuilder typeInfoBuilder = new(classSymbol, typeInfoCache);
    internal ConstructorInfo? Build(IEnumerable<PropertyInfo> classProperties)
    {
        PropertyInfo[] initializerProperties = [..classProperties.Where(p => p is { SetMethod: { } } or { InitMethod: { } })];
        MethodParameterInfo[] parameterInfos = [.. parameterInfoBuilder.Build()];
        
        MethodParameterInfo? initializersObjectParameter = initializerProperties.Length == 0 ? null : new()
        {
            Name = "jsObject",
            IsInjectedInstanceParameter = false,
            Type = InteropTypeInfo.JSObjectTypeInfo
        };

        return new ConstructorInfo
        {
            Name = "ctor",
            Parameters = parameterInfos,
            InitializerObject = initializersObjectParameter,
            Type = typeInfoBuilder.Build(),
            MemberInitializers = [.. initializerProperties],
        };
    }
}