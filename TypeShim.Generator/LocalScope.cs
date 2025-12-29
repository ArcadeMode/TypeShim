namespace TypeShim.Generator;

/// <summary>
/// Tracks variable names for method parameters within a local scope. Used for transparently tracking parameters that may get renamed/reassigned during type conversions.
/// </summary>
internal sealed class LocalScope
{
    private readonly Dictionary<MethodParameterInfo, string> paramNameDict;
    
    internal LocalScope(MethodInfo methodInfo)
    {
        paramNameDict = methodInfo.Parameters.ToDictionary(c => c, c => c.Name);
    }

    internal LocalScope(ConstructorInfo constructorInfo)
    {
        paramNameDict = constructorInfo.Parameters.ToDictionary(c => c, c => c.Name);
    }


    internal string GetAccessorExpression(MethodParameterInfo paramInfo)
    {
        return paramNameDict[paramInfo];
    }

    internal void UpdateAccessorExpression(MethodParameterInfo paramInfo, string newExpression)
    {
        paramNameDict[paramInfo] = newExpression;
    }
}