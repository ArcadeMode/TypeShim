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

        bool isJSExport = memberMethod.GetAttributes().Any(AttributeFacts.IsJSExportAttribute);
        IReadOnlyCollection<MethodParameterInfo> parameters = [.. parameterInfoBuilder.Build()];
        InteropTypeInfo returnType = typeInfoBuilder.Build();

        if (isJSExport)
        {
            foreach (MethodParameterInfo param in parameters)
            {
                if (param.Type.RequiresTypeConversion)
                {
                    throw new NotSupportedJSExportReferenceException($"JSExport methods '{classSymbol.Name}.{memberMethod.Name}' cannot reference classes in its parameter types.");
                }
            }
            if (returnType.RequiresTypeConversion)
            {
                throw new NotSupportedJSExportReferenceException($"JSExport methods '{classSymbol.Name}.{memberMethod.Name}' cannot reference classes in its return type.");
            }
        }

        return new MethodInfo()
        {
            IsStatic = memberMethod.IsStatic,
            Name = memberMethod.Name,
            Parameters = parameters,
            ReturnType = returnType,
            Comment = new CommentInfoBuilder(memberMethod).Build(),
        };
    }
}
