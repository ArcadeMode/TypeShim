using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class TypescriptUserClassProxyRenderer(ClassInfo classInfo, TypeScriptMethodRenderer methodRenderer, TypescriptClassNameBuilder classNameBuilder)
{
    // PURPOSE:
    // - glue between interop interface for a single class instance, enabling dynamic method invocation

    private readonly StringBuilder sb = new();

    internal string Render()
    {
        string proxyClassName = classNameBuilder.GetUserClassProxyName(classInfo);
        string interopInterfaceName = classNameBuilder.GetModuleInteropClassName();
        RenderProxyClass(proxyClassName, interopInterfaceName);


        string staticsClassName = classNameBuilder.GetUserClassStaticsName(classInfo);
        RenderStaticsClass(staticsClassName, interopInterfaceName);
        return sb.ToString();
    }

    private void RenderStaticsClass(string className, string interopInterfaceName)
    {
        string indent = "  ";
        sb.AppendLine($"// Auto-generated TypeScript statics class. Source class: {classInfo.Namespace}.{classInfo.Name}");
        
        sb.AppendLine($"export class {className} {{");
        sb.AppendLine($"{indent}private interop: {interopInterfaceName};");
        sb.AppendLine();
        sb.AppendLine($"{indent}constructor(interop: {interopInterfaceName}) {{");
        sb.AppendLine($"{indent}{indent}this.interop = interop;");
        sb.AppendLine($"{indent}}}");
        sb.AppendLine();
        foreach (MethodInfo methodInfo in classInfo.Methods.Where(m => m.IsStatic))
        {
            sb.AppendLine($"{indent}public {methodRenderer.RenderMethodSignatureForClass(methodInfo)} {{");
            RenderInteropInvocation(indent, methodInfo);

            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }

        foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(p => p.IsStatic))
        {
            MethodInfo? getter = propertyInfo.GetMethod;
            sb.AppendLine($"{indent}public {methodRenderer.RenderPropertyGetterSignatureForClass(getter.WithoutInstanceParameter())} {{");
            RenderInteropInvocation(indent, getter);
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();

            if (propertyInfo.SetMethod is MethodInfo setter)
            {
                sb.AppendLine($"{indent}public {methodRenderer.RenderPropertySetterSignatureForClass(setter.WithoutInstanceParameter())} {{");
                RenderInteropInvocation(indent, setter);
                sb.AppendLine($"{indent}}}");
                sb.AppendLine();
            }
        }
        sb.AppendLine($"}}");
    }

    private void RenderInteropInvocation(string indent, MethodInfo methodInfo)
    {
        string interopInvoke = methodRenderer.RenderMethodCallParametersWithInstanceParameterExpression(methodInfo, "this.instance"); // note: instance parameter will be unused for static methods
        if (classNameBuilder.GetUserClassProxyName(methodInfo.ReturnType) is string proxyClassName) // user class return type, wrap in proxy
        {
            string optionalAwait = methodInfo.ReturnType.IsTaskType ? "await " : string.Empty;
            sb.AppendLine($"{indent}{indent}const res = {optionalAwait}this.interop.{ResolveInteropMethodAccessor(classInfo, methodInfo)}({interopInvoke});");

            if (methodInfo.ReturnType.IsArrayType)
            {
                sb.AppendLine($"{indent}{indent}return res.map(item => {GetNewProxyExpression(methodInfo.ReturnType, proxyClassName, "item")});");
            }
            else
            {
                sb.AppendLine($"{indent}{indent}return {GetNewProxyExpression(methodInfo.ReturnType, proxyClassName, "res")};");
            }
        }
        else // primitive return type or void
        {
            string optionalReturn = methodInfo.ReturnType.ManagedType == KnownManagedType.Void ? string.Empty : "return ";
            sb.AppendLine($"{indent}{indent}{optionalReturn}this.interop.{ResolveInteropMethodAccessor(classInfo, methodInfo)}({interopInvoke});");
        }
    }

    //private void RenderPropertyGetter(string indent, PropertyInfo propertyInfo)
    //{
    //    MethodInfo methodInfo = propertyInfo.GetMethod;
    //    sb.AppendLine($"{indent}public get {propertyInfo.Name}(): {propertyInfo.Type} {{");
    //    RenderInteropInvocation(indent, methodInfo);
    //    sb.AppendLine($"{indent}}}");
    //    sb.AppendLine();
    //}

    //private void RenderPropertySetter(string indent, PropertyInfo propertyInfo)
    //{
    //    MethodInfo methodInfo = propertyInfo.GetMethod;
    //    sb.AppendLine($"{indent}public set {propertyInfo.Name}(): {propertyInfo.Type} {{");
    //    RenderInteropInvocation(indent, methodInfo);
    //    sb.AppendLine($"{indent}}}");
    //    sb.AppendLine();
    //}

    //private void RenderInteropInvocation(string indent, PropertyInfo propertyInfo)
    //{
    //    RenderInteropInvocation(indent, propertyInfo.GetMethod);
    //    if (propertyInfo.SetMethod is MethodInfo setter)
    //    {
    //        RenderInteropInvocation(indent, setter);
    //    }
    //}

    private string GetNewProxyExpression(InteropTypeInfo returnTypeInfo, string proxyClassName, string instanceName)
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

    private void RenderProxyClass(string className, string interopInterfaceName)
    {
        string indent = "  ";
        sb.AppendLine($"// Auto-generated TypeScript proxy class. Source class: {classInfo.Namespace}.{classInfo.Name}");

        sb.AppendLine($"export class {className} implements {classInfo.Name} {{");
        sb.AppendLine($"{indent}private interop: {interopInterfaceName};");
        sb.AppendLine($"{indent}private instance: object;");
        sb.AppendLine();
        sb.AppendLine($"{indent}constructor(instance: object, interop: {interopInterfaceName}) {{");
        sb.AppendLine($"{indent}{indent}this.interop = interop;");
        sb.AppendLine($"{indent}{indent}this.instance = instance;");
        sb.AppendLine($"{indent}}}");
        sb.AppendLine();

        foreach (MethodInfo methodInfo in classInfo.Methods.Where(m => !m.IsStatic))
        {
            sb.AppendLine($"{indent}public {methodRenderer.RenderMethodSignatureForClass(methodInfo.WithoutInstanceParameter())} {{");
            RenderInteropInvocation(indent, methodInfo);
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(p => !p.IsStatic))
        {
            MethodInfo? getter = propertyInfo.GetMethod;
            sb.AppendLine($"{indent}public {methodRenderer.RenderPropertyGetterSignatureForClass(getter.WithoutInstanceParameter())} {{");
            RenderInteropInvocation(indent, getter);
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();

            if (propertyInfo.SetMethod is MethodInfo setter)
            {
                sb.AppendLine($"{indent}public {methodRenderer.RenderPropertySetterSignatureForClass(setter.WithoutInstanceParameter())} {{");
                RenderInteropInvocation(indent, setter);
                sb.AppendLine($"{indent}}}");
                sb.AppendLine();
            }
        }
        sb.AppendLine($"}}");
    }

    private string ResolveInteropMethodAccessor(ClassInfo classInfo, MethodInfo methodInfo)
    {
        return $"{classInfo.Namespace}.{classNameBuilder.GetInteropInterfaceName(classInfo)}.{methodInfo.Name}";
    }

    private string ResolveInteropPropertyAccessor(ClassInfo classInfo, PropertyInfo propertyInfo)
    {
        return $"{classInfo.Namespace}.{classNameBuilder.GetInteropInterfaceName(classInfo)}.{propertyInfo.Name}";
    }
}
