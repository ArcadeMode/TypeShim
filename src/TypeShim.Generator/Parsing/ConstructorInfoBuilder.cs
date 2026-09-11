using Microsoft.CodeAnalysis;
using TypeShim.Generator.Parsing;
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
            Name = "initializer",
            Type = InteropTypeInfo.JSObjectTypeInfo
        };

        ConstructorInfo constructorInfo = new()
        {
            Name = "ctor",
            Parameters = parameterInfos,
            InitializerObject = initializersObjectParameter,
            Type = typeInfoBuilder.Build(),
            MemberInitializers = [.. initializerProperties],
            Comment = new CommentInfoBuilder(memberMethod).Build(initializersObjectParameter),
        };

        if (constructorInfo.HasOptionalParameters
            && constructorInfo.InitializerObject != null
            && constructorInfo.HasRequiredMemberInitializers)
        {
            throw new NotSupportedOptionalParameterException(
                $"Class '{classSymbol.Name}' cannot combine optional constructor parameters with a non-omittable initializer object. " +
                "Make the parameters required, or ensure the initializer has no required members so it can be omitted.");
        }

        return constructorInfo;
    }
}