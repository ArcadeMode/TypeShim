using Microsoft.CodeAnalysis;

namespace TypeShim.Parsing;

internal sealed class ClassInfoBuilder(INamedTypeSymbol classSymbol)
{
    internal ClassInfo Build()
    { 
        List<MethodInfoBuilder> methodInfoBuilders = new();
        foreach (IMethodSymbol methodSymbol in classSymbol.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Ordinary))
        {
            MethodInfoBuilder methodInfoBuilder = new(classSymbol, methodSymbol);
            methodInfoBuilders.Add(methodInfoBuilder);
        }

        // FEATURE: generate property accessors (mostly needs correct receivers on TS end)
        foreach (var member in classSymbol.GetMembers().OfType<IPropertySymbol>())
        { }

        return new ClassInfo
        {
            Namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            Name = classSymbol.Name,
            Methods = methodInfoBuilders.Select(b => b.Build())
        };
    }
}
