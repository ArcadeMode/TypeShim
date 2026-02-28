using TypeShim.Shared;

internal sealed class ConstructorInfo
{
    internal required string Name { get; init; }
    internal required MethodParameterInfo[] Parameters { get; init; }
    internal required MethodParameterInfo? InitializerObject { get; init; }
    internal required PropertyInfo[] MemberInitializers { get; init; }
    internal required InteropTypeInfo Type { get; init; }
    
    internal bool IsParameterless => Parameters.Length == 0;
    internal bool AcceptsInitializer => MemberInitializers.Length > 0;

    internal MethodParameterInfo[] GetParametersIncludingInitializerObject()
    {
        if (InitializerObject is MethodParameterInfo initializer)
        {
            return [.. Parameters, initializer];
        }
        return Parameters;
    }
}
