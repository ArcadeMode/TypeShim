using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptMethodRenderer(ClassInfo classInfo, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sb = new();

    internal string GetRenderedContent()
    {
        return sb.ToString();
    }

    internal void RenderProxyMethod(int depth, MethodInfo methodInfo)
    {
        string indent = new(' ', depth * 2);
        sb.AppendLine($"{indent}public {RenderProxyMethodSignature(methodInfo.WithoutInstanceParameter())} {{");
        RenderMethodBodyContent(depth + 1, methodInfo);
        sb.AppendLine($"{indent}}}");
        sb.AppendLine();

        string RenderProxyMethodSignature(MethodInfo methodInfo)
        {
            string returnType = symbolNameProvider.GetUserClassSymbolNameIfExists(methodInfo.ReturnType, SymbolNameFlags.Proxy) ?? symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType);

            string optionalAsync = methodInfo.ReturnType.IsTaskType ? "async " : string.Empty;
            return $"{optionalAsync}{methodInfo.Name}({GetProxyMethodParameterList(methodInfo)}): {returnType}";
        }
    }

    internal void RenderProxyProperty(int depth, PropertyInfo propertyInfo)
    {
        string indent = new(' ', depth * 2);
        MethodInfo getterInfo = propertyInfo.GetMethod;
        sb.AppendLine($"{indent}public {RenderProxyPropertyGetterSignature(getterInfo.WithoutInstanceParameter())} {{");
        RenderMethodBodyContent(depth + 1, getterInfo);
        sb.AppendLine($"{indent}}}");
        sb.AppendLine();

        if (propertyInfo.SetMethod is MethodInfo setterInfo)
        {
            sb.AppendLine($"{indent}public {RenderProxyPropertySetterSignature(setterInfo.WithoutInstanceParameter())} {{");
            RenderMethodBodyContent(depth + 1, setterInfo);
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }

        string RenderProxyPropertyGetterSignature(MethodInfo methodInfo)
        {
            string returnType = symbolNameProvider.GetUserClassSymbolNameIfExists(methodInfo.ReturnType, SymbolNameFlags.Proxy) ?? symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType);
            return $"get {propertyInfo.Name}({GetProxyMethodParameterList(methodInfo)}): {returnType}";
        }

        string RenderProxyPropertySetterSignature(MethodInfo methodInfo)
        {
            return $"set {propertyInfo.Name}({GetProxyMethodParameterList(methodInfo)})";
        }
    }

    private string GetProxyMethodParameterList(MethodInfo methodInfo)
    {
        return string.Join(", ", methodInfo.MethodParameters.Select(p =>
        {
            SymbolNameFlags flags = ContainsSnapshotCompatibleType(p.Type) ? SymbolNameFlags.ProxySnapshotUnion : SymbolNameFlags.Proxy;
            string returnType = symbolNameProvider.GetUserClassSymbolNameIfExists(p.Type, flags) ?? symbolNameProvider.GetNakedSymbolReference(p.Type);
            return $"{p.Name}: {returnType}";
        }));

        static bool ContainsSnapshotCompatibleType(InteropTypeInfo typeInfo)
        {
            if (typeInfo.IsSnapshotCompatible)
                return true;
            if (typeInfo.TypeArgument is InteropTypeInfo innerTypeInfo)
                return ContainsSnapshotCompatibleType(innerTypeInfo);
            return false;
        }
    }

    internal void RenderMethodBodyContent(int depth, MethodInfo methodInfo)
    {
        RenderManagedObjectConstAssignments(depth, methodInfo);
        RenderInteropInvocation(depth, methodInfo);
    }

    private void RenderManagedObjectConstAssignments(int depth, MethodInfo methodInfo)
    {
        string indent = new(' ', depth * 2);
        foreach (MethodParameterInfo parameterInfo in methodInfo.MethodParameters)
        {
            if (parameterInfo.IsInjectedInstanceParameter || !parameterInfo.Type.RequiresCLRTypeConversion)
                continue;

            // The element type for arrays/tasks or inner type of nullables is what matters for conversion purposes
            InteropTypeInfo paramTargetType = parameterInfo.Type.TypeArgument ?? parameterInfo.Type; 

            if (!paramTargetType.IsTSExport) 
                continue;

            if (symbolNameProvider.GetUserClassSymbolNameIfExists(paramTargetType, SymbolNameFlags.Proxy | SymbolNameFlags.Isolated) is not string proxyClassName)
            {
                throw new ArgumentException($"Invalid conversion-requiring type '{parameterInfo.Type.CLRTypeSyntax}' failed to resolve associated TypeScript proxy name.");
            }

            sb.Append($"{indent}const {GetInteropInvocationVariable(parameterInfo)} = ");
            if (parameterInfo.Type is { IsArrayType: true } or { IsTaskType: true })
            {
                string transformFunction = parameterInfo.Type.IsArrayType ? "map" : "then";
                sb.Append($"{parameterInfo.Name}.{transformFunction}(e => {GetManagedObjectFromProxyExpression(proxyClassName, "e")})");
            }
            else // simple or nullable proxy types
            {
                sb.Append(GetManagedObjectFromProxyExpression(proxyClassName, parameterInfo.Name));
            }
            sb.AppendLine(";");
        }

        static string GetManagedObjectFromProxyExpression(string proxyClassName, string varName)
        {
            return $"{varName} instanceof {proxyClassName} ? {varName}.instance : {varName}";
        }
    }

    private void RenderInteropInvocation(int depth, MethodInfo methodInfo)
    {
        string indent = new(' ', depth * 2);
        string interopInvoke = RenderMethodCallParametersWithInstanceParameterExpression(methodInfo, "this.instance"); // note: instance parameter will be unused for static methods

        InteropTypeInfo returnType = methodInfo.ReturnType;

        if (symbolNameProvider.GetUserClassSymbolNameIfExists(returnType, SymbolNameFlags.Proxy | SymbolNameFlags.Isolated) is string proxyClassName)
        {
            // user class return type, wrap in proxy
            sb.AppendLine($"{indent}const res = this.interop.{ResolveInteropMethodAccessor(classInfo, methodInfo)}({interopInvoke});");

            if (returnType.IsArrayType || returnType.IsTaskType)
            {
                string transformFunction = returnType.IsArrayType ? "map" : "then";
                sb.AppendLine($"{indent}return res.{transformFunction}(e => {GetNewProxyExpression(returnType.TypeArgument!, proxyClassName, "e")});");
            }
            else
            {
                sb.AppendLine($"{indent}return {GetNewProxyExpression(returnType, proxyClassName, "res")};");
            }
        }
        else // primitive return type or void
        {
            string optionalReturn = returnType.ManagedType == KnownManagedType.Void ? string.Empty : "return ";
            sb.AppendLine($"{indent}{optionalReturn}this.interop.{ResolveInteropMethodAccessor(classInfo, methodInfo)}({interopInvoke});");
        }

        string RenderMethodCallParametersWithInstanceParameterExpression(MethodInfo methodInfo, string instanceParameterExpression)
        {
            return string.Join(", ", methodInfo.MethodParameters.Select(p => p.IsInjectedInstanceParameter ? instanceParameterExpression : GetInteropInvocationVariable(p)));
        }

        string ResolveInteropMethodAccessor(ClassInfo classInfo, MethodInfo methodInfo)
        {
            return $"{classInfo.Namespace}.{symbolNameProvider.GetInteropInterfaceName(classInfo)}.{methodInfo.Name}";
        }

        static string GetNewProxyExpression(InteropTypeInfo returnTypeInfo, string proxyClassName, string instanceName)
        {
            if (returnTypeInfo.IsNullableType)
            {
                return $"{instanceName} ? new {proxyClassName}({instanceName}, this.interop) : null";
            }
            else
            {
                return $"new {proxyClassName}({instanceName}, this.interop)";
            }
        }
    }

    private static string GetInteropInvocationVariable(MethodParameterInfo param)
    {
        return param.Type.RequiresCLRTypeConversion && (param.Type.TypeArgument ?? param.Type).IsTSExport ? $"{param.Name}Instance" : param.Name;
    }
}